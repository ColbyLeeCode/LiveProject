

//Send email to Portland Job Placement Director
public async Task<ActionResult> SendEmail(MailMessage message)
{
    string networkUser = "PLACEHOLDER@live.com";//REPLACE WITH VALID VALUE 
    string networkPass = "Pass1234";//REPLACE WITH VALID VALUE 
    if (ModelState.IsValid)
    {
        using (var smtp = new SmtpClient())
        {
            var credential = new NetworkCredential
            {
                UserName = networkUser,
                Password = networkPass
            };
            smtp.Credentials = credential;
            smtp.Host = "smtp-mail.outlook.com";
            smtp.Port = 587;
            smtp.EnableSsl = true;
            await smtp.SendMailAsync(message);
        }
    }
    return View();
}

// POST: JPHires/Create
// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
// more details see http://go.microsoft.com/fwlink/?LinkId=317598.
[HttpPost]
[ValidateAntiForgeryToken]
public ActionResult Create([Bind(Include = "JPHireId,JPStudentId,JPCompanyName,JPJobTitle,JPJobCategory,JPSalary,JPCompanyCity,JPCompanyState,JPSecondJob,JPCareersPage,JPHireDate")] JPHire jPHire)
{
    if (ModelState.IsValid)
    {
        // Grabs the active users ID and uses it to identify the users row in JPStudents table to edit JPGraduated and JPHired from false to true.
        string userID = User.Identity.GetUserId();
        JPStudent jpStudent = db.JPStudents.Where(x => x.ApplicationUserId == userID).FirstOrDefault();
        jpStudent.JPGraduated = true;
        jpStudent.JPHired = true;
        
        //Auto-populating JPHireId, ApplicationUserId, and JPHireDate during user creation.
        jPHire.JPHireId = Guid.NewGuid();
        jPHire.JPHireDate = DateTime.Now;
        jPHire.ApplicationUserId = userID;
        
        db.Entry(jpStudent).State = EntityState.Modified;
        db.JPHires.Add(jPHire);
        db.SaveChanges();

        //Build notification email and assign sender/recipient
        MailMessage message = new MailMessage();
        message.To.Add(new MailAddress("PortlandJobPlacement@learncodinganywhere.com"));
        message.From = new MailAddress("PLACEHOLDER@live.com");//REPLACE WITH VALID VALUE
        message.Subject = "Automated Hiring Alert";
        message.Body = jpStudent.JPName + " has submit a Hiring form. This is an automated notification.";
        message.IsBodyHtml = false;

        //Send notification email to portland jobs director via async task
        HostingEnvironment.QueueBackgroundWorkItem(_ => SendEmail(message));
        
        return RedirectToAction("Index");
    }
    return View(jPHire);
}
