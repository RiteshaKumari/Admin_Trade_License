using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Helpers;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Security;
using AdminSide.Models;
using Razorpay.Api;

namespace AdminSide.Controllers
{
    public class HomeController : Controller
    {
        private Utility ul = new Utility();
        public string EncodeToBase64UrlSafe(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            string base64 = System.Convert.ToBase64String(plainTextBytes);
            return base64.Replace("+", "-").Replace("/", "_").TrimEnd('=');
        }
        public string DecodeFromBase64UrlSafe(string base64UrlSafe)
        {
            try
            {
                string base64 = base64UrlSafe.Replace("-", "+").Replace("_", "/");
                switch (base64.Length % 4)
                {
                    case 2: base64 += "=="; break;
                    case 3: base64 += "="; break;
                }
                var base64EncodedBytes = System.Convert.FromBase64String(base64);
                return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
            }
            catch (FormatException)
            {
                return "Invalid Base64 string";
            }
        }
        public string getName()
        {
            HttpCookie cookie = Request.Cookies["MobileNumber"];
            if (cookie != null)
            {
                string encodedValue = cookie.Value;
                string decodedMobileNumber = DecodeFromBase64UrlSafe(encodedValue);

                SqlParameter[] parameters1 = new SqlParameter[]
                {
                   new SqlParameter("@mobileNumber", decodedMobileNumber),
                };

                var value = ul.func_ExecuteScalar("rit_getName", parameters1);
                if (value != null)
                {
                    return value.ToString();
                }
                else
                {
                    return "";
                }
            }
            else
            {
                return "";
            }
         }
        public ActionResult Index()
        {
            HttpCookie cookie = Request.Cookies["MobileNumber"];
            if (cookie != null)
            {
               return RedirectToAction("newLicense");
            }
            else
            {
                DataSet DS1 = ul.fn_DataSet("rit_getDesignation");
                var SCS1 = DS1.Tables[0];
                var Res1 = SCS1.AsEnumerable().Select(s => new index
                {
                    designation = s.Field<string>("assign_designation")

                }).ToList();
                ViewBag.designation = new SelectList(Res1, "designation", "designation");
                return View();
            }     
        }
        public void create_otp()
        {
            Random random = new Random();
            string combination = "0123456789";
            StringBuilder captcha = new StringBuilder();
            for (int i = 0; i < 5; i++)
                captcha.Append(combination[random.Next(combination.Length)]);
            Session["otp"] = captcha.ToString();
            string message = "Your OTP is " + Session["otp"].ToString();
            string mobileNo = Session["mob"].ToString();
            SendSms(mobileNo, message);
        }
        public void SendSms(string mobileNo, string message)
        {
            string apiUrl = "https://pmc.bihar.gov.in/service/pmcsendsms.asmx/SendSms?mobileNo=" + mobileNo + "&message=" + message + "&key=7110EDA4D09E062AA5E4A390B0A572AC0D2C0220";
            System.Net.WebRequest request = System.Net.HttpWebRequest.Create(apiUrl);
            System.Net.HttpWebResponse response = (System.Net.HttpWebResponse)request.GetResponse();
            Stream s = (Stream)response.GetResponseStream();
            StreamReader readStream = new StreamReader(s);
            string dataString = readStream.ReadToEnd();
            Session["otp_generated_time"] = DateTime.Now;
            response.Close();
            s.Close();
            readStream.Close();
        }

        [HttpPost]
        public ActionResult Index(index model)
        {
            DataSet DS1 = ul.fn_DataSet("rit_getDesignation");
            var SCS1 = DS1.Tables[0];
            var Res1 = SCS1.AsEnumerable().Select(s => new index
            {
                designation = s.Field<string>("assign_designation")
            }).ToList();
            ViewBag.designation = new SelectList(Res1, "designation", "designation");

            if (ModelState.IsValid)
            {
                SqlParameter[] param = new SqlParameter[]
                 {
                 new SqlParameter("@phone", model.phone),
                 new SqlParameter("@designation", model.designation),
                 new SqlParameter("@ipaddress", model.IPAddress)
                 };
                object resultObj = ul.func_ExecuteScalar("rit_checkForOTP", param);
                int result = resultObj != null ? Convert.ToInt32(resultObj) : 0;
                if (result > 0)
                {
                    Session["mob"] = model.phone;
                    Session.Timeout = 60;
                    create_otp();
                    Session["otpTime"] = DateTime.Now;
                    return RedirectToAction("OTP");
                }
                else
                {
                    TempData["error"] = "Invalid Details!";
                    return View();
                }
            }
            else
            {
                return View();
            }
        }

        [Route("OTP")]
        public ActionResult OTP()
        {
            if (Session["mob"] == null)
            {
                return RedirectToAction("Index");
            }
            TempData["mobile"] = Session["mob"].ToString();
            ViewBag.Timer = "Code in 01:00"; 
            return View();
        }

        [HttpPost]
        [Route("OTP")]
        public JsonResult OTP(otp model)
        {
            TempData.Keep("mobile");

            if (Session["otpTime"] != null)
            {
                DateTime otpGeneratedTime = (DateTime)Session["otpTime"];
                TimeSpan timeElapsed = DateTime.Now - otpGeneratedTime;

                if (timeElapsed.TotalSeconds > 60)
                {
                    return Json(new { success = false, message = "OTP has expired. Please request a new one.", returnValue = -99 });
                }
            }

            SqlParameter[] parameters = new SqlParameter[]
            {
        new SqlParameter("@mobile", TempData["mobile"]),
        new SqlParameter("@OTP", model.myotp),
        new SqlParameter("@datetime", DateTime.Now),
        new SqlParameter("@result", SqlDbType.Int) { Direction = ParameterDirection.Output }  // Added OUTPUT parameter
            };

            object result = ul.func_ExecuteScalar("citizenOTPCount", parameters);
            int returnValue = parameters[3].Value != DBNull.Value ? Convert.ToInt32(parameters[3].Value) : -99;  // Retrieve OUTPUT value

            if (model.myotp.Length == 5)
            {
                if (model.myotp == Session["otp"].ToString() && returnValue != -1)
                {
                    Session["otpv"] = "1";
                    HttpCookie mobileCookie = new HttpCookie("MobileNumber");
                    mobileCookie.Value = EncodeToBase64UrlSafe(Session["mob"].ToString());
                    mobileCookie.Expires = DateTime.Now.AddDays(1);
                    Response.Cookies.Add(mobileCookie);


                    return Json(new { success = true, redirectUrl = Url.Action("Dashboard"), message = "OTP validated successfully!", returnValue });
                }
                else
                {
                    string errorMessage = returnValue == -1
                        ? "You have only 3 attempts. Please wait before trying again as your submit button is inactive."
                        : "Please enter a valid 5-digit OTP.";

                    return Json(new { success = false, message = errorMessage, returnValue });
                }
            }
            else
            {
                return Json(new { success = false, message = "Invalid OTP.", returnValue = -99 });
            }
        }

        [HttpPost]
        public ActionResult resendOTP()
        {
            TempData.Keep("mobile");
            create_otp();
            return Json(new { success = true, message = "OTP resent successfully." });
        }

        [Route("Dashboard")]
        public ActionResult Dashboard()
        {
            HttpCookie cookie = Request.Cookies["MobileNumber"];
            if (cookie != null)
            {
                string name = getName();
                TempData["name"] = name;
                return View();
            }
            else
            {
                return RedirectToAction("Index");
            }
        }

        [Route("newLicense")]
        public ActionResult newLicense()
        {
            HttpCookie cookie = Request.Cookies["MobileNumber"];
            if (cookie != null)
            {
                string name = getName();
                TempData["name"] = name;

                DataSet DS2 = ul.fn_DataSet("rit_SelectFirmType");
                var SCS2 = DS2.Tables[0];
                var Res2 = SCS2.AsEnumerable().Select(s => new FirmClass
                {
                    firm_type = s.Field<string>("firm_type"),
                    firm_id = s.Field<object>("id") != DBNull.Value ? Convert.ToInt32(s.Field<object>("id")) : 0
                }).ToList();
                ViewBag.SelectFirmType = new SelectList(Res2, "firm_id", "firm_type");

                DataSet DS5 = ul.fn_DataSet("rit_SelectLisenceYear");
                var SCS5 = DS5.Tables[0];
                var Res5 = SCS5.AsEnumerable().Select(s => new licenseyear
                {
                    Lisence_id = s.Field<object>("id") != DBNull.Value ? Convert.ToInt32(s.Field<object>("id")) : 0,
                    FormattedYear = $"{s.Field<int>("license_year")} (Year)"
                }).ToList();
                ViewBag.SelectLisenceYear = new SelectList(Res5, "Lisence_id", "FormattedYear");
        

                DataSet DS3 = ul.fn_DataSet("rit_SelectCategory");
                var SCS3 = DS3.Tables[0];
                var Res3 = SCS3.AsEnumerable().Select(s => new CategoryClass
                {
                    category_name = s.Field<string>("category_name"),
                    category_id = s.Field<object>("id") != DBNull.Value ? Convert.ToInt32(s.Field<object>("id")) : 0

                }).ToList();
                ViewBag.SelectCategory = new SelectList(Res3, "category_id", "category_name");

                DataSet DS4 = ul.fn_DataSet("rit_SelectBusinessPremisesOwner");
                var SCS4 = DS4.Tables[0];
                var Res4 = SCS4.AsEnumerable()
                 .Select(s => new BussinessOwner
                 {
                     occupency_type = s.Field<string>("occupency_type"),
                     occupency_id = s.Field<object>("id") != DBNull.Value ? Convert.ToInt32(s.Field<object>("id")) : 0
                 })
                 .ToList();

                ViewBag.SelectBusinessPremisesOwner = new SelectList(Res4, "occupency_id", "occupency_type");

                return View();
            }
            else
            {
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [Route("newLicense")]
        public ActionResult newLicense(NewLicense model)
        {
            HttpCookie custmobileCookie = new HttpCookie("custmobileNumber");
            string custmobile = model.mobile_no.ToString();
            custmobileCookie.Value = EncodeToBase64UrlSafe(custmobile);
            custmobileCookie.Expires = DateTime.Now.AddDays(1);
            Response.Cookies.Add(custmobileCookie);
            string name = getName();
            TempData["name"] = name;
            if (ModelState.IsValid)
            {
                SqlParameter[] param = new SqlParameter[]
                 {
                  new SqlParameter("@firm_name", model.firm_name),
                  new SqlParameter("@consumer_Name", model.Consumer_Name),
                  new SqlParameter("@mobile_no", model.mobile_no),
                  new SqlParameter("@firm_type", model.firm_type),
                  new SqlParameter("@holding_no", model.holding_no),
                  new SqlParameter("@Businessward_id", model.Businessward_id),
                  new SqlParameter("@business_address", model.business_address),
                  new SqlParameter("@DOE", model.DOE),
                  new SqlParameter("@business_name",model.business_name),
                  new SqlParameter("@BusinessSize", model.BusinessSize),
                  new SqlParameter("@license_duration", model.license_duration),
                  new SqlParameter("@total_Price", model.totalRate),
                  new SqlParameter("@occupancy_type_id", model.occupancy_type_id)
                };
                var isValid = ul.func_ExecuteScalar("rit_CounterLicenseApplication", param);
                if (isValid != null)
                {
                    TempData["totalRate"] = model.totalRate;
                    return RedirectToAction("ConsumerDocumentUpload");
                }
                else
                {
                    HttpCookie cookie = Request.Cookies["MobileNumber"];
                    if (cookie != null)
                    {
                            DataSet DS2 = ul.fn_DataSet("rit_SelectFirmType");
                            var SCS2 = DS2.Tables[0];
                            var Res2 = SCS2.AsEnumerable().Select(s => new FirmClass
                            {
                                firm_type = s.Field<string>("firm_type"),
                                firm_id = s.Field<object>("id") != DBNull.Value ? Convert.ToInt32(s.Field<object>("id")) : 0

                            }).ToList();
                            ViewBag.SelectFirmType = new SelectList(Res2, "firm_id", "firm_type");

                        DataSet DS4 = ul.fn_DataSet("rit_SelectBusinessPremisesOwner");
                        var SCS4 = DS4.Tables[0];
                        var Res4 = SCS4.AsEnumerable()
                         .Select(s => new BussinessOwner
                         {
                             occupency_type = s.Field<string>("occupency_type"),
                             occupency_id = s.Field<object>("id") != DBNull.Value ? Convert.ToInt32(s.Field<object>("id")) : 0
                         })
                         .ToList();

                        ViewBag.SelectBusinessPremisesOwner = new SelectList(Res4, "occupency_id", "occupency_type");
                        DataSet DS3 = ul.fn_DataSet("rit_SelectCategory");
                            var SCS3 = DS3.Tables[0];
                            var Res3 = SCS3.AsEnumerable().Select(s => new CategoryClass
                            {
                                category_name = s.Field<string>("category_name"),
                                category_id = s.Field<object>("id") != DBNull.Value ? Convert.ToInt32(s.Field<object>("id")) : 0

                            }).ToList();
                            ViewBag.SelectCategory = new SelectList(Res3, "category_id", "category_name");

                            DataSet DS5 = ul.fn_DataSet("rit_SelectLisenceYear");
                            var SCS5 = DS5.Tables[0];
                            var Res5 = SCS5.AsEnumerable().Select(s => new licenseyear
                            {
                                Lisence_id = s.Field<object>("id") != DBNull.Value ? Convert.ToInt32(s.Field<object>("id")) : 0,
                                FormattedYear = $"{s.Field<int>("license_year")} (Year)"
                            }).ToList();
                            ViewBag.SelectLisenceYear = new SelectList(Res5, "Lisence_id", "FormattedYear");

                            ViewBag.error = null;
                            ViewBag.error = "Oops, Your data not saved successfully !";
                            model.business_name = 0;
                            model.BusinessSize = 0m;
                            model.license_duration = "";
                            return View(model);
                    }
                    else
                    {
                        model.business_name = 0;
                        model.BusinessSize = 0m;
                        model.license_duration = "";
                        ViewBag.error = "Something went wrong !";
                        return View();
                    }
                }
            }
            else
            {
                model.business_name = 0;
                model.BusinessSize = 0m;
                model.totalRate = 0;
                model.license_duration = string.Empty;

                ModelState.Remove("business_name");
                ModelState.Remove("BusinessSize");
                ModelState.Remove("totalRate");
                ModelState.Remove("license_duration");

                HttpCookie cookie = Request.Cookies["MobileNumber"];
                if (cookie != null)
                {
                    DataSet DS2 = ul.fn_DataSet("rit_SelectFirmType");
                    var SCS2 = DS2.Tables[0];
                    var Res2 = SCS2.AsEnumerable().Select(s => new FirmClass
                    {
                        firm_type = s.Field<string>("firm_type"),
                        firm_id = s.Field<object>("id") != DBNull.Value ? Convert.ToInt32(s.Field<object>("id")) : 0

                    }).ToList();
                    ViewBag.SelectFirmType = new SelectList(Res2, "firm_id", "firm_type");

                      DataSet DS4 = ul.fn_DataSet("rit_SelectBusinessPremisesOwner");
                    var SCS4 = DS4.Tables[0];
                    var Res4 = SCS4.AsEnumerable()
                     .Select(s => new BussinessOwner
                     {
                         occupency_type = s.Field<string>("occupency_type"),
                         occupency_id = s.Field<object>("id") != DBNull.Value ? Convert.ToInt32(s.Field<object>("id")) : 0
                     })
                     .ToList();

                    ViewBag.SelectBusinessPremisesOwner = new SelectList(Res4, "occupency_id", "occupency_type");
                    DataSet DS3 = ul.fn_DataSet("rit_SelectCategory");
                    var SCS3 = DS3.Tables[0];
                    var Res3 = SCS3.AsEnumerable().Select(s => new CategoryClass
                    {
                        category_name = s.Field<string>("category_name"),
                        category_id = s.Field<object>("id") != DBNull.Value ? Convert.ToInt32(s.Field<object>("id")) : 0

                    }).ToList();
                    ViewBag.SelectCategory = new SelectList(Res3, "category_id", "category_name");

                    DataSet DS5 = ul.fn_DataSet("rit_SelectLisenceYear");
                    var SCS5 = DS5.Tables[0];
                    var Res5 = SCS5.AsEnumerable().Select(s => new licenseyear
                    {
                        Lisence_id = s.Field<object>("id") != DBNull.Value ? Convert.ToInt32(s.Field<object>("id")) : 0,
                        FormattedYear = $"{s.Field<int>("license_year")} (Year)"
                    }).ToList();
                    ViewBag.SelectLisenceYear = new SelectList(Res5, "Lisence_id", "FormattedYear");

                    ViewBag.error = "Oops, Your data not saved successfully!";
                        TempData["value"] = model;
                        return View(model);
                    }
                    else
                    {
                        ViewBag.error = "Something went wrong!";
                        return View(model);
                    }
            }
        }

        [Route("ConsumerDocumentUpload")]
        public ActionResult ConsumerDocumentUpload()
        {
            string name = getName();
            TempData["name"] = name;
            TempData.Keep("totalRate");
            HttpCookie cookie = Request.Cookies["MobileNumber"];
            if (cookie != null)
            {
                HttpCookie cookie1 = Request.Cookies["custmobileNumber"];
                string encodedValue = cookie1.Value;
                string decodedMobileNumber = DecodeFromBase64UrlSafe(encodedValue);
                SqlParameter[] parameters = new SqlParameter[]
                {
                  new SqlParameter("@mobileNo", decodedMobileNumber),
                };
                DataSet DS3 = ul.fn_DataSet("rit_getCounterPaymentForm", parameters);
                var SCS3 = DS3.Tables[0]; 
                var Res3 = SCS3.AsEnumerable().Select(s => new PaymentForm
                {
                    application_no = s.Field<string>("application_no"),
                }).LastOrDefault();
                //TempData.Keep("totalRate");

                ViewBag.application_no = Res3.application_no;

                return View();
            }
            else
            {
                return RedirectToAction("Index");
            }
        }

        private int SaveFileNamesToDatabase(string mobileNumber, List<Tuple<string, string, int, string>> filesWithDetails, string IPAddress)
        {
            string name = getName();
            TempData["name"] = name;
            if (ModelState.IsValid)
            {
                foreach (var fileDetail in filesWithDetails)
                {
                    SqlParameter[] parameters = new SqlParameter[]
                    {
                       new SqlParameter("@mobileNumber", mobileNumber),
                       new SqlParameter("@fileName", fileDetail.Item1),
                       new SqlParameter("@documentType", fileDetail.Item2),
                       new SqlParameter("@isMandatory", fileDetail.Item3),
                       new SqlParameter("@doc_name", fileDetail.Item4),
                       new SqlParameter("@ip_address", IPAddress)
                    };

                    var value = ul.func_ExecuteNonQuery("rit_saveCustomerdocument", parameters);
                    if (value <= 0)
                    {
                        ViewBag.ErrorMessage = "Error occurred while saving the documents.";
                        return 0;
                    }
                }
                ModelState.Clear();
                TempData.Keep("totalRate");
                return 1;
            }
            else
            {
                ViewBag.ErrorMessage = "Enter valid inputs!";
                return 0;
            }
        }

        [HttpPost]
        [Route("ConsumerDocumentUpload")]
        public ActionResult ConsumerDocumentUpload(HttpPostedFileBase aadhaarfile, HttpPostedFileBase voterfile, HttpPostedFileBase panfile, HttpPostedFileBase applicationform, HttpPostedFileBase rentagreement, HttpPostedFileBase photo, string IPAddress)
        {
            string name = getName();
            TempData["name"] = name;
            HttpCookie cookie1 = Request.Cookies["custmobileNumber"];
            string encodedValue = cookie1.Value;
            string decodedMobileNumber = DecodeFromBase64UrlSafe(encodedValue);
            //ViewBag.MobileNumber = decodedMobileNumber;

            string folderPath = Server.MapPath("/Content/Customer_File/");
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            List<Tuple<string, string, int, string>> savedFilesWithDetails = new List<Tuple<string, string, int, string>>();

            // File size limit (2MB)
            int maxFileSize = 2 * 1024 * 1024;

            // Handle Aadhaar file upload (Non-mandatory)
            if (aadhaarfile != null && aadhaarfile.ContentLength > 0 && aadhaarfile.ContentLength <= maxFileSize)
            {
                string aadhaarFileName = $"{decodedMobileNumber}_aadhaar_{Path.GetFileName(aadhaarfile.FileName)}";
                string aadhaarFilePath = Path.Combine(folderPath, aadhaarFileName);
                try
                {
                    aadhaarfile.SaveAs(aadhaarFilePath);
                    savedFilesWithDetails.Add(Tuple.Create(aadhaarFileName, "Aadhaar", 0, "ID PROOF"));  // Non-mandatory
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Error occurred while saving Aadhaar file.";
                    // Log the error (ex)
                    return View();
                }
            }

            // Handle Voter file upload (Non-mandatory)
            if (voterfile != null && voterfile.ContentLength > 0 && voterfile.ContentLength <= maxFileSize)
            {
                string voterFileName = $"{decodedMobileNumber}_voter_{Path.GetFileName(voterfile.FileName)}";
                string voterFilePath = Path.Combine(folderPath, voterFileName);
                try
                {
                    voterfile.SaveAs(voterFilePath);
                    savedFilesWithDetails.Add(Tuple.Create(voterFileName, "Voter", 0, "ID PROOF"));  // Non-mandatory
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Error occurred while saving Voter file.";
                    // Log the error (ex)
                    return View();
                }
            }

            // Handle PAN file upload (Non-mandatory)
            if (panfile != null && panfile.ContentLength > 0 && panfile.ContentLength <= maxFileSize)
            {
                string panFileName = $"{decodedMobileNumber}_pan_{Path.GetFileName(panfile.FileName)}";
                string panFilePath = Path.Combine(folderPath, panFileName);
                try
                {
                    panfile.SaveAs(panFilePath);
                    savedFilesWithDetails.Add(Tuple.Create(panFileName, "PAN", 0, "ID PROOF"));  // Non-mandatory
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Error occurred while saving PAN file.";
                    // Log the error (ex)
                    return View();
                }
            }

            // Handle Application Form upload (Mandatory)
            if (applicationform != null && applicationform.ContentLength > 0 && applicationform.ContentLength <= maxFileSize)
            {
                string applicationFileName = $"{decodedMobileNumber}_application_{Path.GetFileName(applicationform.FileName)}";
                string applicationFilePath = Path.Combine(folderPath, applicationFileName);
                try
                {
                    applicationform.SaveAs(applicationFilePath);
                    savedFilesWithDetails.Add(Tuple.Create(applicationFileName, "Application Form", 1, "FORM"));  // Mandatory
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Error occurred while saving Application Form.";
                    // Log the error (ex)
                    return View();
                }
            }

            // Handle Rent Agreement upload (Mandatory)
            if (rentagreement != null && rentagreement.ContentLength > 0 && rentagreement.ContentLength <= maxFileSize)
            {
                string rentAgreementFileName = $"{decodedMobileNumber}_rent_{Path.GetFileName(rentagreement.FileName)}";
                string rentAgreementFilePath = Path.Combine(folderPath, rentAgreementFileName);
                try
                {
                    rentagreement.SaveAs(rentAgreementFilePath);
                    savedFilesWithDetails.Add(Tuple.Create(rentAgreementFileName, "Rent Agreement", 1, "DOCUMENT"));  // Mandatory
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Error occurred while saving Rent Agreement.";
                    // Log the error (ex)
                    return View();
                }
            }

            // Handle Photo upload (Mandatory)
            if (photo != null && photo.ContentLength > 0 && photo.ContentLength <= maxFileSize)
            {
                string photoFileName = $"{decodedMobileNumber}_photo_{Path.GetFileName(photo.FileName)}";
                string photoFilePath = Path.Combine(folderPath, photoFileName);
                try
                {
                    photo.SaveAs(photoFilePath);
                    savedFilesWithDetails.Add(Tuple.Create(photoFileName, "Photo", 1, "PHOTO"));  // Mandatory
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Error occurred while saving Photo.";
                    // Log the error (ex)
                    return View();
                }
            }

            // Now save the file names, document types, and mandatory status to the database
            int result = SaveFileNamesToDatabase(decodedMobileNumber, savedFilesWithDetails, IPAddress);
            if (result > 0)
            {
                TempData.Keep("totalRate");
                TempData["Success"] = "Documents uploaded successfully!";
                return RedirectToAction("ConsumerPaymentForm");
            }
            else
            {
                TempData["Error"] = "Documents were not uploaded successfully!";
                return View();
            }
        }

        [Route("ConsumerPaymentForm")]
        public ActionResult ConsumerPaymentForm()
        {
            string name = getName();
            TempData["name"] = name;
            HttpCookie cookie = Request.Cookies["MobileNumber"];
            if (cookie != null)
            {
                HttpCookie cookie1 = Request.Cookies["custmobileNumber"];
                string encodedValue = cookie1.Value;
                string decodedMobileNumber = DecodeFromBase64UrlSafe(encodedValue);
             
            SqlParameter[] parameters1 = new SqlParameter[]
            {
                   new SqlParameter("@mobileNumber", decodedMobileNumber),
            };

            var value = ul.func_ExecuteScalar("rit_checkCustomerdocument", parameters1);
            int result = Convert.ToInt32(value);
            if (result <= 0)
            {
                ViewBag.ErrorMessage = "Error occurred while saving the documents.";
                return RedirectToAction("ConsumerDocumentUpload");
            }
            else
            {
                ViewBag.CategoryList = new List<SelectListItem>
                 {
                    new SelectListItem { Text = "CHEQUE", Value = "CHEQUE"},
                    new SelectListItem { Text = "CARD", Value = "CARD" },
                    new SelectListItem { Text = "Demand Draft", Value = "demand-draft"},
                    new SelectListItem { Text = "NEFT/IMPS", Value = "NEFTorIMPS"},
                    new SelectListItem { Text = "RTGS", Value = "RTGS"},
                   // new SelectListItem { Text = "UPI", Value = "UPI"}
                 };
            
                SqlParameter[] parameters = new SqlParameter[]
                    {
                       new SqlParameter("@mobileNo", decodedMobileNumber),
                    };
                    DataSet DS3 = ul.fn_DataSet("rit_getCounterPaymentForm", parameters);
                    var SCS3 = DS3.Tables[0];
                    var Res3 = SCS3.AsEnumerable().Select(s => new PaymentForm
                    {
                        firm_name = s.Field<string>("firm_name"),
                        mobile_no = s.Field<long>("mobile_no"),
                        ward_id = s.Field<long>("ward_id"),
                        bussiness_address = s.Field<string>("bussiness_address"),
                        owner_name = s.Field<string>("consumer_name"),
                        doe = s.Field<DateTime>("doe"),
                        BussinessType1 = s.Field<string>("BussinessType1"),
                        sqare_feet = s.Field<string>("sqare_feet"),
                        application_no = s.Field<string>("application_no"),
                        total_Price = s.Field<decimal?>("total_Price") ?? 0 
                    }).LastOrDefault();
                 
                  ///  TempData["totalRates"] = Res3.total_Price;
                    TempData["totalRates"] = 5000;
                    TempData["applicationNO"] = Res3.application_no;
                    ViewBag.ConsumerPaymentFormData = Res3;
                    return View();
             }
            }
            else
            {
                return RedirectToAction("Index");
            }
        }

        [Route("ConsumerPaymentForm")]
        [HttpPost]
        public ActionResult ConsumerPaymentForm(string ipaddress, PaymentFormModel model)
        {
            ViewBag.CategoryList = new List<SelectListItem>
                 {
                    new SelectListItem { Text = "CHEQUE", Value = "CHEQUE"},
                    new SelectListItem { Text = "CARD", Value = "CARD" },
                    new SelectListItem { Text = "NEFT/IMPS", Value = "NEFTorIMPS"},
                    new SelectListItem { Text = "RTGS", Value = "RTGS"}
                    //new SelectListItem { Text = "UPI", Value = "UPI"}
                 };
            
                    HttpCookie cookie = Request.Cookies["MobileNumber"];
                    if (cookie != null)
                    {
                       if(model.paymentmode == "CHEQUE")
                    {
                    TempData.Keep("totalRates");
                    TempData.Keep("applicationNO");
                    ViewBag.TotalAmounts = TempData["totalRates"];

                
                    HttpCookie cookie1 = Request.Cookies["custmobileNumber"];
                    string encodedValue = cookie1.Value;
                    string decodedMobileNumber = DecodeFromBase64UrlSafe(encodedValue);
                    //SqlParameter[] parameters = new SqlParameter[]
                    //{
                    //       new SqlParameter("@mobileNo", decodedMobileNumber)
                    //};
                    //DataSet DS3 = ul.fn_DataSet("rit_fetchConsumer", parameters);
                    //var SCS3 = DS3.Tables[0];
                    //var Res3 = SCS3.AsEnumerable().Select(s => new NewLicenseApplication
                    //{
                    //    holding_no = s.Field<string>("holding_no"),
                    //    email_id = s.Field<string>("email_id")
                    //}).LastOrDefault();

                    SqlParameter[] parameters1 = new SqlParameter[]
                    {
                          new SqlParameter("@application_no", TempData["applicationNO"]),
                          new SqlParameter("@mobileNo", decodedMobileNumber),
                          new SqlParameter("@IPAddress", ipaddress),
                          new SqlParameter("@remarks", model.Narration),
                          new SqlParameter("@paymentMode", model.paymentmode),
                          new SqlParameter("@bank_name", model.CHEQUEbankName),
                          new SqlParameter("@branch_name", model.CHEQUEbankBranch),
                          new SqlParameter("@cheque_no", model.ChequeNo),
                          new SqlParameter("@cheque_date", model.ChequeDate),
                          new SqlParameter("@totalPrice", ViewBag.TotalAmounts)
                        
                    };
                    //  DataSet DS4 = ul.fn_DataSet("rit_ConsumerRequestSave", parameters1);
                    var value = ul.func_ExecuteNonQuery("rit_saveCounterPaymentSuccess", parameters1);
                    int amountInPaise = (int)(Convert.ToDecimal(ViewBag.TotalAmounts) * 100);
                    TempData["TotalAmt"] = amountInPaise;
                    if (value > 0)
                    {

                        return Json(new { redirectUrl = Url.Action("CHEQUECustomerReceipt") });
                    }
                    else
                    {
                        TempData["saveTransactionfail"] = "** Oops ! your payment information not saved";
                        return Json(new { success = false, message = "Operation failed, please try again." });
                    }
                }
                else
                {
                    return Json(new { success = false, message = "Operation failed, please try again." });//------Temporary---------  
                }
                        
                    }
                    else
                    {
                        TempData["savetransactionfail"] = "** oops ! your cookie lost.";
                return Json(new { success = false, message = "Operation failed, please try again." });
            }                
              
        }

        [Route("CounterOnlinePayment")]
        [HttpPost]
        public ActionResult CounterOnlinePayment(string ipaddress, PaymentFormModel model)
        {
            ViewBag.CategoryList = new List<SelectListItem>
                 {
                    new SelectListItem { Text = "CHEQUE", Value = "CHEQUE"},
                    new SelectListItem { Text = "CARD", Value = "CARD" },
                    new SelectListItem { Text = "NEFT/IMPS", Value = "NEFTorIMPS"},
                    new SelectListItem { Text = "RTGS", Value = "RTGS"},
                    new SelectListItem { Text = "UPI", Value = "UPI"}
                 };

            if (model.paymentmode == "CARD" && model.Narration != null)
            {
                HttpCookie cookie = Request.Cookies["MobileNumber"];
                if (cookie != null)
                {
                    TempData.Keep("totalRates");
                    ViewBag.TotalAmounts = TempData["totalRates"];

                    HttpCookie cookie1 = Request.Cookies["custmobileNumber"];
                    string encodedValue = cookie1.Value;
                    string decodedMobileNumber = DecodeFromBase64UrlSafe(encodedValue);
                    SqlParameter[] parameters = new SqlParameter[]
                    {
                           new SqlParameter("@mobileNo", decodedMobileNumber)
                    };
                    DataSet DS3 = ul.fn_DataSet("rit_fetchConsumer", parameters);
                    var SCS3 = DS3.Tables[0];
                    var Res3 = SCS3.AsEnumerable().Select(s => new NewLicenseApplication
                    {
                        holding_no = s.Field<string>("holding_no"),
                        email_id = s.Field<string>("email_id")
                    }).LastOrDefault();

                    SqlParameter[] parameters1 = new SqlParameter[]
                    {
                          new SqlParameter("@mobileNo", decodedMobileNumber),
                          new SqlParameter("@IPAddress", ipaddress),
                          new SqlParameter("@TotalAmount", ViewBag.TotalAmounts),
                          new SqlParameter("@PropertyNo", Res3.holding_no),
                          new SqlParameter("@Email", Res3.email_id),
                    };
                    //  DataSet DS4 = ul.fn_DataSet("rit_ConsumerRequestSave", parameters1);
                    var value = ul.func_ExecuteNonQuery("rit_ConsumerRequestSave", parameters1);
                    int amountInPaise = (int)(Convert.ToDecimal(ViewBag.TotalAmounts) * 100);
                    TempData["TotalAmt"] = amountInPaise;
                    if (value > 0)
                    {
                        //  string key = "rzp_live_0nlwx1xD3328Ee";
                        string key = "rzp_test_Xl5GKpLJQb6PLe";
                        //  string secret = "YIfnMIOU8nICFE9mJI5L8UOP";
                        string secret = "iXjaphOc4Q36Vg8FPMPcwfAJ";
                        RazorpayClient client = new RazorpayClient(key, secret);
                        Dictionary<string, object> options = new Dictionary<string, object>
                        {
                            { "amount", amountInPaise }, // Amount in the smallest currency unit (50,000 paise = 500 INR)
                            { "currency", "INR" },
                            { "receipt", "order_rcptid_12" }
                        };
                        Order order = client.Order.Create(options);
                        //TempData["order"] = Newtonsoft.Json.JsonConvert.SerializeObject(order);
                        //TempData.Keep("order");
                        TempData["order"] = Newtonsoft.Json.JsonConvert.SerializeObject(order.Attributes);
                        TempData.Keep("order");
                        return Json(new { success = true, redirectUrl = Url.Action("RequestHandler") });
                        // return RedirectToAction("PaymentPage");
                    }
                    else
                    {
                        TempData["saveTransactionfail"] = "** Oops ! your payment information not saved";
                        //  return View();
                        return Json(new { success = false, message = "** Oops ! your payment information not saved" });
                    }
                }
                else
                {
                    TempData["savetransactionfail"] = "** oops ! your cookie lost.";
                    //  return view();
                    return Json(new { success = false, message = "** oops ! your cookie lost" });
                }

            }
            else
            {
                return Json(new { success = false, message = "** Fill Details Correctly !" });
            }
        }

        [Route("RequestHandler")]
        public ActionResult RequestHandler()
        {
            HttpCookie oldCookie = Request.Cookies["MobileNumber"];
            if (TempData["order"] != null)
            {
                string orderJson = TempData["order"].ToString();
                var orderData = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(orderJson);
                string orderId = orderData["id"].ToString();
                ViewBag.OrderId = orderId;
            }
            TempData.Keep("totalRates");
            TempData.Keep("TotalAmt");
            TempData.Keep("order");

            HttpCookie cookie = Request.Cookies["MobileNumber"];
            if (cookie != null)
            {
                HttpCookie cookie1 = Request.Cookies["custmobileNumber"];
                string encodedValue = cookie1.Value;
                string decodedMobileNumber = DecodeFromBase64UrlSafe(encodedValue);
                SqlParameter[] parameters = new SqlParameter[]
                {
                new SqlParameter("@mobileNo", decodedMobileNumber)
                };
                DataSet DS3 = ul.fn_DataSet("rit_fetchConsumerdetail", parameters);
                var SCS3 = DS3.Tables[0];
                var Res3 = SCS3.AsEnumerable().Select(s => new NewLicenseApplication
                {
                    mobile_no = s.Field<long>("mobile_no"),
                    application_no = s.Field<string>("application_no"),
                    Consumer_Name = s.Field<string>("consumer_name"),
                    email_id = s.Field<string>("email_id") ?? "No Email Available"
                }).LastOrDefault();
                ViewBag.Detail = Res3;
                TempData["mobile_no"] = Res3.mobile_no;
                TempData["application_no"] = Res3.application_no;
                TempData["Consumer_Name"] = Res3.Consumer_Name;
                TempData["email_id"] = string.IsNullOrEmpty(Res3.email_id) ? "No Email Available" : Res3.email_id;


                if (cookie != null)
                {
                    cookie.Expires = DateTime.Now.AddDays(-1);
                    Response.Cookies.Add(cookie);
                }

                HttpCookie newcookie = new HttpCookie("nextNumber")
                {
                    Value = EncodeToBase64UrlSafe(decodedMobileNumber),
                    Expires = DateTime.Now.AddMinutes(8)
                };
                Response.Cookies.Add(newcookie);
            }
            else
            {
                return RedirectToAction("Index");
            }
            return View();
        }

        [Route("PaymentSuccess")]
        [HttpPost]
        public ActionResult PaymentSuccess(string razorpay_payment_id, string razorpay_order_id, string razorpay_signature, string IPAddress)
        {
            HttpCookie cookie = Request.Cookies["nextNumber"];
            if (cookie != null)
            {

                TempData.Keep("totalRates");
                TempData.Keep("application_no");
                TempData.Keep("email_id");
                TempData.Keep("Consumer_Name");
                TempData.Keep("mobile_no");
                string keyId = "rzp_test_Xl5GKpLJQb6PLe";
                string keySecret = "iXjaphOc4Q36Vg8FPMPcwfAJ";
                try
                {
                    // Create a Razorpay client
                    RazorpayClient client = new RazorpayClient(keyId, keySecret);

                    // Create a dictionary with the payment attributes
                    Dictionary<string, string> attributes = new Dictionary<string, string>
            {
                { "razorpay_payment_id", razorpay_payment_id },
                { "razorpay_order_id", razorpay_order_id },
                { "razorpay_signature", razorpay_signature }
            };

                    // Verify the payment signature
                    Utils.verifyPaymentSignature(attributes);

                    // If the verification passes, the payment is successful
                    ViewBag.Message = "Payment verified successfully!";
                    SqlParameter[] parameters1 = new SqlParameter[]
                           {
                    new SqlParameter("@mobileNo", TempData["mobile_no"]),
                    new SqlParameter("@application_no", TempData["application_no"]),
                    new SqlParameter("@email", TempData["email_id"]),
                    new SqlParameter("@IPAddress", IPAddress),
                    new SqlParameter("@totalPrice", TempData["totalRates"]),
                    new SqlParameter("@fullName", TempData["Consumer_Name"]),
                    new SqlParameter("@razorpay_order_id", razorpay_order_id),
                           };
                    //  DataSet DS4 = ul.fn_DataSet("rit_ConsumerRequestSave", parameters1);
                    var value = ul.func_ExecuteNonQuery("rit_savePaymentSuccess", parameters1);
                    if (value > 0)
                    {
                        return Json(new { success = true, redirectUrl = Url.Action("CustomerReceipt") });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Failure" });
                    }
                }
                catch (Exception ex)
                {
                    ViewBag.Message = "Payment verification failed: " + ex.Message;
                    return Json(new { success = false, message = "Failure" });
                }

            }
            return Json(new { success = false, message = "Failure" });
        }

        [HttpPost]
        [Route("PaymentFailure")]
        public ActionResult PaymentFailure(string order_id, string IPAddress)
        {
            HttpCookie cookie = Request.Cookies["nextnumber"];
            if (cookie != null)
            {
                TempData.Keep("totalRates");
                TempData.Keep("email_id");
                TempData.Keep("Consumer_Name");
                TempData.Keep("mobile_no");
                try
                {
                    SqlParameter[] parameters1 = new SqlParameter[]
                    {
                    new SqlParameter("@email", TempData["email_id"]),
                    new SqlParameter("@IPAddress", IPAddress),
                    new SqlParameter("@totalPrice", TempData["totalRates"]),
                    new SqlParameter("@fullName", TempData["Consumer_Name"]),
                    new SqlParameter("@razorpay_order_id", order_id),
                    };
                    var value = ul.func_ExecuteNonQuery("rit_savePaymentFailure", parameters1);
                    if (value > 0)
                    {
                        cookie.Expires = DateTime.Now.AddDays(-1);
                        Response.Cookies.Add(cookie);
                        return Json(new { success = true, redirectUrl = Url.Action("Index") });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Failure" });
                    }
                }
                catch (Exception ex)
                {
                    ViewBag.Message = "Payment verification failed: " + ex.Message;
                    return Json(new { success = false, message = "Failure" });
                }

            }
            return Json(new { success = false, message = "Failure" });
        }

        [Route("CustomerReceipt")]
        public ActionResult CustomerReceipt()
        {
            HttpCookie cookie = Request.Cookies["nextNumber"];
            if (cookie != null)
            {
                string encodedValue = cookie.Value;
                string decodedMobileNumber = DecodeFromBase64UrlSafe(encodedValue);
                SqlParameter[] parameters = new SqlParameter[]
                {
                   new SqlParameter("@mobileNo", decodedMobileNumber),
                };
                DataSet DS3 = ul.fn_DataSet("rit_getConsumerPaymentReceipt", parameters);
                var SCS3 = DS3.Tables[0];
                var Res3 = SCS3.AsEnumerable().Select(s => new PaymentForm
                {
                    firm_name = s.Field<string>("firm_name") ?? string.Empty,
                    Consumer_name = s.Field<string>("consumer_name") ?? string.Empty,
                    mobile_no = s.Field<long?>("mobile_no") ?? 0,
                    ward_id = s.Field<long?>("ward_id") ?? 0,
                    address = s.Field<string>("address") ?? string.Empty,
                    owner_name = s.Field<string>("owner_name") ?? string.Empty,
                    doe = s.Field<DateTime?>("doe") ?? DateTime.MinValue,
                    receipt_no = s.Field<string>("receipt_no") ?? string.Empty,
                    timestamp = s.Field<DateTime?>("timestamp") ?? DateTime.MinValue,
                    sqare_feet = s.Field<string>("sqare_feet") ?? string.Empty,
                    application_no = s.Field<string>("application_no") ?? string.Empty,
                    total_payable_amount = s.Field<decimal?>("total_payable_amount") ?? 0m,
                }).LastOrDefault();

                ViewBag.ConsumerPaymentReceipt = Res3;
                //cookie.Expires = DateTime.Now.AddDays(-1);
                //Response.Cookies.Add(cookie);
            }
            return View();
        }

        [Route("CHEQUECustomerReceipt")]
        public ActionResult CHEQUECustomerReceipt()
        {
            HttpCookie cookie = Request.Cookies["MobileNumber"];
            if (cookie != null)
            {
                HttpCookie cookie1 = Request.Cookies["custmobileNumber"];
                string encodedValue = cookie1.Value;
                string decodedMobileNumber = DecodeFromBase64UrlSafe(encodedValue);
                SqlParameter[] parameters = new SqlParameter[]
                {
                   new SqlParameter("@mobileNo", decodedMobileNumber),
                };
                DataSet DS3 = ul.fn_DataSet("rit_getCounterPaymentReceipt", parameters);
                var SCS3 = DS3.Tables[0];
                var Res3 = SCS3.AsEnumerable().Select(s => new PaymentForm
                {
                    firm_name = s.Field<string>("firm_name") ?? string.Empty,
                    Consumer_name = s.Field<string>("consumer_name") ?? string.Empty,
                    mobile_no = s.Field<long?>("mobile_no") ?? 0,
                    ward_id = s.Field<long?>("ward_id") ?? 0,
                    address = s.Field<string>("address") ?? string.Empty,
                 //   owner_name = s.Field<string>("owner_name") ?? string.Empty,
                    doe = s.Field<DateTime?>("doe") ?? DateTime.MinValue,
                    receipt_no = s.Field<string>("receipt_no") ?? string.Empty,
                    timestamp = s.Field<DateTime?>("timestamp") ?? DateTime.MinValue,
                    sqare_feet = s.Field<string>("sqare_feet") ?? string.Empty,
                    application_no = s.Field<string>("application_no") ?? string.Empty,
                    total_payable_amount = s.Field<decimal?>("total_payable_amount") ?? 0m,
                }).LastOrDefault();

                ViewBag.ConsumerPaymentReceipt = Res3;
                //cookie.Expires = DateTime.Now.AddDays(-1);
                //Response.Cookies.Add(cookie);
            }
            return View();
        }

        [Route("getTotalRate")]
        [HttpPost]
        public JsonResult getTotalRate(int CategoryID, int License_Year, int bussiness_size)
        {
            string name = getName();
            TempData["name"] = name;
            SqlParameter[] param = new SqlParameter[]
               {
                 new SqlParameter("@bussiness_size", bussiness_size),
                 new SqlParameter("@License_Year", License_Year),
                 new SqlParameter("@CategoryID", CategoryID)

               };
            var isValid = (decimal)ul.func_ExecuteScalar("rit_getTotalRate", param);
            if (isValid > 0)
            {
                return Json(new { success = true, totalRate = isValid });
            }
            else
            {
                return Json(new { success = false, message = "Invalid data." });
            }
        }

        [Route("PID_Changed")]
        [HttpPost]
        public ActionResult PID_Changed(string pid)
        {
            string name = getName();
            TempData["name"] = name;
            SqlParameter[] param = new SqlParameter[]
            {
              new SqlParameter("@pid", pid)
            };

            DataSet DSA = ul.fn_DataSet("rit_getBussinessAddressWard", param);
            var Category = DSA.Tables[0];

            var Res2 = Category.AsEnumerable().Select(s => new
            {
                business_address = s.Field<string>("address"),
                Businessward_id = s.Field<string>("ward_no")
            }).FirstOrDefault();

            if (Res2 != null)
            {
                return Json(new { success = true, businessdata = Res2 }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { success = false, message = "Something went wrong!" }, JsonRequestBehavior.AllowGet);
            }
        }

        [Route("trackStatusDetail")]
        public ActionResult trackStatusDetail()
        {
            return View();
        }
       
        [Route("tradePayNow")]
        public ActionResult tradePayNow()
        {
            return View();
        }

        [HttpPost]
        [Route("Logout")]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();

            if (Request.Cookies["nextNumber"] != null)
            {
                var pnmskrbrCookie = new HttpCookie("nextNumber")
                {
                    Expires = DateTime.Now.AddDays(-1)
                };
                Response.Cookies.Add(pnmskrbrCookie);
            }

            if (Request.Cookies["custmobileNumber"] != null)
            {
                var pnmskrbrCookie = new HttpCookie("custmobileNumber")
                {
                    Expires = DateTime.Now.AddDays(-1)
                };
                Response.Cookies.Add(pnmskrbrCookie);
            }

            if (Request.Cookies["MobileNumber"] != null)
            {
                var customerTokenCookie = new HttpCookie("MobileNumber")
                {
                    Expires = DateTime.Now.AddDays(-1)
                };
                Response.Cookies.Add(customerTokenCookie);
            }
            Session.Clear();
            Session.Abandon();
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }
    }
}