

DROP DATABASE IF EXISTS applicantsss;
CREATE DATABASE applicantsss;
USE applicantsss;


--  APPLICANT SIDE TABLES
select * from applicant_profiles;

-- 1. APPLICANTS 
CREATE TABLE applicants (
    id              INT PRIMARY KEY AUTO_INCREMENT,
    first_name      VARCHAR(100) NOT NULL,
    last_name       VARCHAR(100) NOT NULL,
    email           VARCHAR(150) UNIQUE NOT NULL,
    password        VARCHAR(255) NOT NULL,
    mobile_number   VARCHAR(20),
    date_of_birth   DATE,
    created_at      TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- 2. APPLICANT ACCOUNT STATUS (active/inactive)
CREATE TABLE applicant_account_status (
    id              INT PRIMARY KEY AUTO_INCREMENT,
    applicant_id    INT NOT NULL UNIQUE,
    is_active       TINYINT(1) DEFAULT 1,
    deactivated_by  VARCHAR(150),
    deactivated_at  TIMESTAMP NULL,
    reason          VARCHAR(300),
    FOREIGN KEY (applicant_id) REFERENCES applicants(id) ON DELETE CASCADE
);


-- HR / ADMIN SIDE TABLES -----------------


-- 3. ROLES (with all permissions for HR Manager/Admin)
CREATE TABLE roles (
    id                    INT PRIMARY KEY AUTO_INCREMENT,
    role_name             VARCHAR(50) NOT NULL UNIQUE,
    description           VARCHAR(200),
    can_accept_applicant  TINYINT(1) DEFAULT 0,
    can_manage_vacancies  TINYINT(1) DEFAULT 0,
    can_review_applicants TINYINT(1) DEFAULT 0,
    can_screen            TINYINT(1) DEFAULT 0,
    can_schedule          TINYINT(1) DEFAULT 0,
    can_evaluate          TINYINT(1) DEFAULT 0,
    can_generate_reports  TINYINT(1) DEFAULT 0,
    can_manage_users      TINYINT(1) DEFAULT 0,
    can_view_audit        TINYINT(1) DEFAULT 0,
    is_active             TINYINT(1) DEFAULT 1
);

INSERT INTO roles
    (id, role_name, description, can_accept_applicant, can_manage_vacancies,
     can_review_applicants, can_screen, can_schedule, can_evaluate,
     can_generate_reports, can_manage_users, can_view_audit)
VALUES
(1, 'Admin',       'Full system access - All permissions',           1, 1, 1, 1, 1, 1, 1, 1, 1),
(2, 'HR Manager',  'Can approve final decisions and manage reports', 1, 1, 1, 1, 1, 1, 1, 0, 1),
(3, 'HR Staff',    'Day-to-day recruitment operations',              0, 0, 1, 1, 1, 1, 0, 0, 0);

-- 4. ADMINS (legacy - for backward compatibility)
CREATE TABLE admins (
    id       INT PRIMARY KEY AUTO_INCREMENT,
    username VARCHAR(100) UNIQUE NOT NULL,
    password VARCHAR(255) NOT NULL,
    role     VARCHAR(50)
);

INSERT INTO admins (username, password, role) VALUES
('admin',   'admin123', 'Admin'),
('hrstaff', 'hr123',    'HR Staff');

-- 5. HR USERS (proper table with role_id - for HR Manager/Admin module)
CREATE TABLE hr_users (
    id          INT PRIMARY KEY AUTO_INCREMENT,
    username    VARCHAR(100) NOT NULL UNIQUE,
    password    VARCHAR(255) NOT NULL,
    full_name   VARCHAR(200) NOT NULL,
    email       VARCHAR(150),
    role_id     INT NOT NULL,
    is_active   TINYINT(1) DEFAULT 1,
    created_at  TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    last_login  TIMESTAMP NULL,
    FOREIGN KEY (role_id) REFERENCES roles(id)
);

INSERT INTO hr_users (username, password, full_name, email, role_id, is_active) VALUES
('admin',       'admin123',     'System Administrator', 'admin@company.com',       1, 1),
('hrmanager',   'hrmanager123', 'Maria Santos',         'maria.santos@company.com', 2, 1),
('sysadmin',    'admin123',     'John Reyes',           'john.reyes@company.com',   1, 1),
('hrstaff1',    'hr123',        'Ana Garcia',           'ana.garcia@company.com',   3, 1),
('hrstaff2',    'hr123',        'Luis Martinez',        'luis.martinez@company.com',3, 1),
('hrstaff3',    'hr123',        'Rosa Rivera',          'rosa.rivera@company.com',  3, 1);

-- ============================================================
--  MAINTENANCE TABLES
-- ============================================================

-- 6. DEPARTMENTS
CREATE TABLE departments (
    id        INT PRIMARY KEY AUTO_INCREMENT,
    name      VARCHAR(100) NOT NULL UNIQUE,
    is_active TINYINT(1) DEFAULT 1
);

INSERT INTO departments (id, name) VALUES
(1, 'IT Department'),
(2, 'Human Resources'),
(3, 'Finance'),
(4, 'Administration'),
(5, 'Operations');

-- 7. POSITIONS
CREATE TABLE positions (
    id            INT PRIMARY KEY AUTO_INCREMENT,
    title         VARCHAR(150) NOT NULL,
    department_id INT NOT NULL,
    is_active     TINYINT(1) DEFAULT 1,
    FOREIGN KEY (department_id) REFERENCES departments(id)
);

INSERT INTO positions (title, department_id) VALUES
('Software Developer',    1), ('IT Support Specialist',   1),
('HR Assistant',          2), ('HR Officer',              2),
('Accounting Staff',      3), ('Finance Analyst',         3),
('Administrative Aide',   4), ('Operations Staff',        5);

-- 8. EMPLOYMENT TYPES
CREATE TABLE employment_types (
    id        INT PRIMARY KEY AUTO_INCREMENT,
    name      VARCHAR(80) NOT NULL UNIQUE,
    is_active TINYINT(1) DEFAULT 1
);

INSERT INTO employment_types (name) VALUES
('Full-time'), ('Part-time'), ('Contractual'), ('Project-based'), ('Internship');

-- 9. REQUIREMENT TYPES
CREATE TABLE requirement_types (
    id        INT PRIMARY KEY AUTO_INCREMENT,
    name      VARCHAR(150) NOT NULL UNIQUE,
    is_active TINYINT(1) DEFAULT 1
);

INSERT INTO requirement_types (name) VALUES
('Resume / CV'),
('Valid Government ID'),
('Transcript of Records'),
('NBI Clearance'),
('Certificate of Employment'),
('Training Certificates'),
('Birth Certificate'),
('Medical Certificate');

-- 10. INTERVIEW TYPES
CREATE TABLE interview_types (
    id        INT PRIMARY KEY AUTO_INCREMENT,
    name      VARCHAR(100) NOT NULL UNIQUE,
    is_active TINYINT(1) DEFAULT 1
);

INSERT INTO interview_types (name) VALUES
('Initial Interview'), ('Technical Interview'), ('HR Interview'), ('Final Interview'), ('Panel Interview');

-- 11. ASSESSMENT TYPES
CREATE TABLE assessment_types (
    id        INT PRIMARY KEY AUTO_INCREMENT,
    name      VARCHAR(100) NOT NULL UNIQUE,
    is_active TINYINT(1) DEFAULT 1
);

INSERT INTO assessment_types (name) VALUES
('Written Exam'), ('Skills Test'), ('Personality Test'), ('Aptitude Test'), ('Technical Assessment');

-- ============================================================
--  CORE APPLICATION TABLES
-- ============================================================

-- 12. JOB VACANCIES
CREATE TABLE job_vacancies (
    id              INT PRIMARY KEY AUTO_INCREMENT,
    title           VARCHAR(150) NOT NULL,
    department_id   INT NOT NULL,
    employment_type VARCHAR(50),
    slots           INT DEFAULT 1,
    qualifications  TEXT,
    description     TEXT,
    status          VARCHAR(20) DEFAULT 'Open',
    posted_at       TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    closed_at       TIMESTAMP NULL,
    FOREIGN KEY (department_id) REFERENCES departments(id)
);

INSERT INTO job_vacancies (id, title, department_id, employment_type, slots, qualifications, status) VALUES
(1, 'Software Developer',    1, 'Full-time', 3, 'BS Computer Science / IT, C#, SQL, Fresh grad OK',  'Open'),
(2, 'HR Assistant',          2, 'Full-time', 1, 'BS Psychology / HRM, Good communication, MS Office',  'Open'),
(3, 'Accounting Staff',      3, 'Full-time', 2, 'BS Accountancy, CPA preferred, Bookkeeping',          'Open'),
(4, 'Administrative Aide',   4, 'Part-time', 2, 'Any 4-year course, Filing and documentation skills',  'Open'),
(5, 'IT Support Specialist', 1, 'Full-time', 1, 'BS IT / Computer Engineering, Hardware, Networking',  'Closed');

-- 13. VACANCY REQUIREMENTS
CREATE TABLE vacancy_requirements (
    id                  INT PRIMARY KEY AUTO_INCREMENT,
    job_vacancy_id      INT NOT NULL,
    requirement_type_id INT NOT NULL,
    is_required         TINYINT(1) DEFAULT 1,
    UNIQUE KEY uq_vac_req (job_vacancy_id, requirement_type_id),
    FOREIGN KEY (job_vacancy_id)      REFERENCES job_vacancies(id)    ON DELETE CASCADE,
    FOREIGN KEY (requirement_type_id) REFERENCES requirement_types(id)
);

INSERT INTO vacancy_requirements (job_vacancy_id, requirement_type_id)
SELECT jv.id, rt.id FROM job_vacancies jv CROSS JOIN requirement_types rt WHERE jv.status = 'Open';

-- 14. APPLICANT PROFILES
CREATE TABLE applicant_profiles (
    id                    INT PRIMARY KEY AUTO_INCREMENT,
    applicant_id          INT NOT NULL UNIQUE,
    middle_name           VARCHAR(100),
    gender                VARCHAR(20),
    civil_status          VARCHAR(30),
    nationality           VARCHAR(80) DEFAULT 'Filipino',
    province              VARCHAR(100),
    city                  VARCHAR(100),
    barangay              VARCHAR(100),
    street                VARCHAR(150),
    zip_code              VARCHAR(10),
    highest_degree        VARCHAR(100),
    school                VARCHAR(150),
    course                VARCHAR(150),
    year_graduated        YEAR,
    skills                TEXT,
    work_company          VARCHAR(150),
    work_position         VARCHAR(100),
    work_duration         VARCHAR(100),
    work_responsibilities TEXT,
    updated_at            TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (applicant_id) REFERENCES applicants(id) ON DELETE CASCADE
);

-- 15. APPLICATIONS
CREATE TABLE applications (
    id                   INT PRIMARY KEY AUTO_INCREMENT,
    applicant_id         INT NOT NULL,
    job_vacancy_id       INT NOT NULL,
    status               VARCHAR(30) DEFAULT 'Draft',
    expected_salary      DECIMAL(10,2),
    preferred_start_date DATE,
    employment_type_pref VARCHAR(50),
    referral_source      VARCHAR(100),
    cover_letter         TEXT,
    submitted_at         TIMESTAMP NULL,
    created_at           TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UNIQUE KEY uq_applicant_job (applicant_id, job_vacancy_id),
    FOREIGN KEY (applicant_id)   REFERENCES applicants(id)   ON DELETE CASCADE,
    FOREIGN KEY (job_vacancy_id) REFERENCES job_vacancies(id) ON DELETE CASCADE
);

-- 16. APPLICANT DOCUMENTS
CREATE TABLE applicant_documents (
    id                  INT PRIMARY KEY AUTO_INCREMENT,
    application_id      INT NOT NULL,
    requirement_type_id INT NOT NULL,
    file_name           VARCHAR(255),
    file_path           VARCHAR(500),
    status              VARCHAR(30) DEFAULT 'Missing',
    hr_remarks          VARCHAR(300),
    uploaded_at         TIMESTAMP NULL,
    updated_at          TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (application_id)      REFERENCES applications(id)      ON DELETE CASCADE,
    FOREIGN KEY (requirement_type_id) REFERENCES requirement_types(id)
);

-- 17. APPLICATION STATUS HISTORY
CREATE TABLE application_status_history (
    id             INT PRIMARY KEY AUTO_INCREMENT,
    application_id INT NOT NULL,
    status         VARCHAR(30) NOT NULL,
    remarks        TEXT,
    status_reason  VARCHAR(300),
    changed_by     VARCHAR(100),
    changed_at     TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (application_id) REFERENCES applications(id) ON DELETE CASCADE
);

-- 18. SCREENING RESULTS
CREATE TABLE screening_results (
    id             INT PRIMARY KEY AUTO_INCREMENT,
    application_id INT NOT NULL UNIQUE,
    result         VARCHAR(20),
    remarks        TEXT,
    screened_by    VARCHAR(150),
    screened_at    TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (application_id) REFERENCES applications(id) ON DELETE CASCADE
);

-- 19. INTERVIEW SCHEDULES
CREATE TABLE interview_schedules (
    id             INT PRIMARY KEY AUTO_INCREMENT,
    application_id INT NOT NULL,
    interview_type VARCHAR(80),
    scheduled_date DATE NOT NULL,
    scheduled_time TIME NOT NULL,
    mode           VARCHAR(50),
    location       VARCHAR(200),
    interviewer    VARCHAR(150),
    status         VARCHAR(30) DEFAULT 'Scheduled',
    created_at     TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (application_id) REFERENCES applications(id) ON DELETE CASCADE
);

-- 20. INTERVIEW EVALUATIONS
CREATE TABLE interview_evaluations (
    id             INT PRIMARY KEY AUTO_INCREMENT,
    interview_id   INT NOT NULL UNIQUE,
    score          DECIMAL(5,2),
    remarks        TEXT,
    result         VARCHAR(20),
    recommendation TEXT,
    evaluated_by   VARCHAR(150),
    evaluated_at   TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (interview_id) REFERENCES interview_schedules(id) ON DELETE CASCADE
);

-- 21. HIRING DECISIONS (Enhanced for HR Manager/Admin)
CREATE TABLE hiring_decisions (
    id             INT PRIMARY KEY AUTO_INCREMENT,
    application_id INT NOT NULL UNIQUE,
    decision       VARCHAR(20) NOT NULL,
    remarks        TEXT,
    start_date     DATE,
    offer_details  TEXT,
    decided_by     VARCHAR(150) NOT NULL,
    decided_at     TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (application_id) REFERENCES applications(id) ON DELETE CASCADE
);

-- 22. AUDIT TRAIL (Enhanced for HR Manager/Admin)
CREATE TABLE audit_trail (
    id             INT PRIMARY KEY AUTO_INCREMENT,
    user_type      VARCHAR(30),
    username       VARCHAR(150),
    user_id        INT,
    action         VARCHAR(200),
    table_name     VARCHAR(100),
    record_id      INT,
    user_action    VARCHAR(200),
    action_details TEXT,
    details        TEXT,
    logged_at      TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    INDEX idx_action (action),
    INDEX idx_table (table_name),
    INDEX idx_logged (logged_at)
);

-- ============================================================
--  VIEWS FOR HR MANAGER/ADMIN DASHBOARDS
-- ============================================================

-- Application summary (Applicant side)
CREATE OR REPLACE VIEW vw_application_summary AS
SELECT
    a.id                                        AS application_id,
    ap.id                                       AS applicant_id,
    CONCAT(ap.first_name, ' ', ap.last_name)   AS applicant_name,
    ap.email,
    jv.title                                    AS job_title,
    d.name                                      AS department,
    jv.employment_type,
    a.status                                    AS application_status,
    a.submitted_at,
    a.created_at,
    (SELECT COUNT(*) FROM applicant_documents ad
     WHERE ad.application_id = a.id
       AND ad.status = 'Missing')               AS missing_docs_count
FROM applications a
JOIN applicants ap    ON ap.id  = a.applicant_id
JOIN job_vacancies jv ON jv.id  = a.job_vacancy_id
JOIN departments d    ON d.id   = jv.department_id;

-- Missing documents
CREATE OR REPLACE VIEW vw_missing_documents AS
SELECT
    ad.application_id,
    rt.name      AS document_name,
    ad.status,
    ad.hr_remarks
FROM applicant_documents ad
JOIN requirement_types rt ON rt.id = ad.requirement_type_id
WHERE ad.status = 'Missing';

-- Full HR applicant report
CREATE OR REPLACE VIEW vw_hr_applicant_report AS
SELECT
    a.id                                            AS application_id,
    CONCAT('APP-', LPAD(a.id, 4, '0'))             AS app_code,
    CONCAT(ap.first_name, ' ', ap.last_name)       AS applicant_name,
    ap.email,
    ap.mobile_number,
    jv.title                                        AS job_title,
    d.name                                          AS department,
    jv.employment_type,
    a.status                                        AS application_status,
    a.submitted_at,
    a.created_at,
    (SELECT COUNT(*) FROM applicant_documents ad
     WHERE ad.application_id = a.id
       AND ad.status = 'Missing')                   AS missing_docs,
    sr.result                                       AS screening_result,
    hd.decision                                     AS hiring_decision
FROM applications a
JOIN applicants ap     ON ap.id  = a.applicant_id
JOIN job_vacancies jv  ON jv.id  = a.job_vacancy_id
JOIN departments d     ON d.id   = jv.department_id
LEFT JOIN screening_results sr ON sr.application_id = a.id
LEFT JOIN hiring_decisions  hd ON hd.application_id = a.id;

-- Pending applications
CREATE OR REPLACE VIEW vw_pending_applications AS
SELECT
    CONCAT('APP-', LPAD(a.id, 4, '0'))             AS app_code,
    CONCAT(ap.first_name, ' ', ap.last_name)       AS applicant_name,
    ap.email,
    jv.title                                        AS job_title,
    a.submitted_at,
    DATEDIFF(NOW(), a.submitted_at)                 AS days_pending
FROM applications a
JOIN applicants ap    ON ap.id = a.applicant_id
JOIN job_vacancies jv ON jv.id = a.job_vacancy_id
WHERE a.status = 'Submitted'
ORDER BY a.submitted_at ASC;

-- Hiring results
CREATE OR REPLACE VIEW vw_hiring_results AS
SELECT
    CONCAT('APP-', LPAD(a.id, 4, '0'))             AS app_code,
    CONCAT(ap.first_name, ' ', ap.last_name)       AS applicant_name,
    ap.email,
    jv.title                                        AS job_title,
    d.name                                          AS department,
    a.status,
    hd.decision,
    hd.remarks,
    hd.start_date,
    hd.decided_by,
    hd.decided_at
FROM applications a
JOIN applicants ap    ON ap.id  = a.applicant_id
JOIN job_vacancies jv ON jv.id  = a.job_vacancy_id
JOIN departments d    ON d.id   = jv.department_id
LEFT JOIN hiring_decisions hd ON hd.application_id = a.id
WHERE a.status IN ('Accepted', 'Rejected', 'On Hold');

-- Missing requirements report
CREATE OR REPLACE VIEW vw_missing_requirements_report AS
SELECT
    CONCAT('APP-', LPAD(a.id, 4, '0'))             AS app_code,
    CONCAT(ap.first_name, ' ', ap.last_name)       AS applicant_name,
    ap.email,
    jv.title                                        AS job_title,
    rt.name                                         AS missing_document,
    a.status                                        AS application_status
FROM applicant_documents ad
JOIN applications a    ON a.id  = ad.application_id
JOIN applicants ap     ON ap.id = a.applicant_id
JOIN job_vacancies jv  ON jv.id = a.job_vacancy_id
JOIN requirement_types rt ON rt.id = ad.requirement_type_id
WHERE ad.status = 'Missing'
  AND a.status NOT IN ('Draft', 'Withdrawn', 'Rejected');

-- Interview report
CREATE OR REPLACE VIEW vw_interview_report AS
SELECT
    CONCAT(ap.first_name, ' ', ap.last_name)       AS applicant_name,
    ap.email,
    jv.title                                        AS job_title,
    is2.interview_type,
    is2.scheduled_date,
    is2.scheduled_time,
    is2.mode,
    is2.location,
    is2.interviewer,
    is2.status                                      AS interview_status,
    ie.score,
    ie.result                                       AS eval_result,
    ie.remarks                                      AS eval_remarks
FROM interview_schedules is2
JOIN applications a    ON a.id  = is2.application_id
JOIN applicants ap     ON ap.id = a.applicant_id
JOIN job_vacancies jv  ON jv.id = a.job_vacancy_id
LEFT JOIN interview_evaluations ie ON ie.interview_id = is2.id
ORDER BY is2.scheduled_date DESC;

-- HR Dashboard Summary View
CREATE OR REPLACE VIEW vw_hr_dashboard_summary AS
SELECT
    'Pending Applications' AS metric,
    COUNT(*) AS count
FROM applications
WHERE status NOT IN ('Accepted', 'Rejected', 'Withdrawn')
UNION ALL
SELECT 'Pending Final Decisions', COUNT(*)
FROM applications
WHERE status = 'For Final Review'
UNION ALL
SELECT 'Accepted Applicants', COUNT(*)
FROM applications
WHERE status = 'Accepted'
UNION ALL
SELECT 'Open Vacancies', COUNT(*)
FROM job_vacancies
WHERE status = 'Open'
UNION ALL
SELECT 'Scheduled Interviews', COUNT(*)
FROM interview_schedules
WHERE status = 'Scheduled' AND scheduled_date >= CURDATE();

-- Pending Final Decisions View (for HR Manager)
CREATE OR REPLACE VIEW vw_pending_final_decisions AS
SELECT
    a.id AS application_id,
    CONCAT('APP-', LPAD(a.id, 4, '0')) AS app_code,
    CONCAT(ap.first_name, ' ', ap.last_name) AS applicant_name,
    ap.email,
    jv.title AS job_title,
    d.name AS department,
    a.status,
    a.submitted_at,
    (SELECT MAX(changed_at) FROM application_status_history WHERE application_id = a.id) AS last_updated
FROM applications a
JOIN applicants ap ON ap.id = a.applicant_id
JOIN job_vacancies jv ON jv.id = a.job_vacancy_id
JOIN departments d ON d.id = jv.department_id
WHERE a.status = 'For Final Review'
ORDER BY a.submitted_at ASC;

-- Active Users View (for User Management)
CREATE OR REPLACE VIEW vw_active_users AS
SELECT
    u.id,
    u.username,
    u.full_name,
    u.email,
    r.role_name AS role,
    u.is_active AS status,
    u.created_at,
    u.last_login
FROM hr_users u
JOIN roles r ON r.id = u.role_id
ORDER BY u.created_at DESC;

-- Audit Log Detail View (for Audit Trail)
CREATE OR REPLACE VIEW vw_audit_log_detail AS
SELECT
    at.id,
    at.user_type,
    at.username,
    at.user_id,
    at.action,
    at.table_name,
    at.record_id,
    at.user_action,
    at.action_details,
    at.details,
    at.logged_at,
    CASE 
        WHEN at.action LIKE '%Create%' THEN 'Create'
        WHEN at.action LIKE '%Update%' THEN 'Update'
        WHEN at.action LIKE '%Delete%' THEN 'Delete'
        WHEN at.action LIKE '%Login%' THEN 'Login'
        WHEN at.action LIKE '%Logout%' THEN 'Logout'
        WHEN at.action LIKE '%Approve%' THEN 'Approve'
        WHEN at.action LIKE '%Reject%' THEN 'Reject'
        ELSE 'Other'
    END AS action_type
FROM audit_trail at
ORDER BY at.logged_at DESC;

-- Recruitment Statistics View
CREATE OR REPLACE VIEW vw_recruitment_stats AS
SELECT
    jv.id AS vacancy_id,
    jv.title AS job_title,
    d.name AS department,
    COUNT(DISTINCT a.id) AS total_applicants,
    SUM(CASE WHEN a.status = 'Draft' THEN 1 ELSE 0 END) AS draft,
    SUM(CASE WHEN a.status = 'Submitted' THEN 1 ELSE 0 END) AS submitted,
    SUM(CASE WHEN a.status = 'Under Review' THEN 1 ELSE 0 END) AS under_review,
    SUM(CASE WHEN a.status = 'Shortlisted' THEN 1 ELSE 0 END) AS shortlisted,
    SUM(CASE WHEN a.status = 'For Interview' THEN 1 ELSE 0 END) AS for_interview,
    SUM(CASE WHEN a.status = 'For Assessment' THEN 1 ELSE 0 END) AS for_assessment,
    SUM(CASE WHEN a.status = 'For Final Review' THEN 1 ELSE 0 END) AS for_final_review,
    SUM(CASE WHEN a.status = 'Accepted' THEN 1 ELSE 0 END) AS accepted,
    SUM(CASE WHEN a.status = 'Rejected' THEN 1 ELSE 0 END) AS rejected,
    SUM(CASE WHEN a.status = 'Withdrawn' THEN 1 ELSE 0 END) AS withdrawn
FROM job_vacancies jv
LEFT JOIN applications a ON a.job_vacancy_id = jv.id
JOIN departments d ON d.id = jv.department_id
GROUP BY jv.id;

-- ============================================================
--  STORED PROCEDURES FOR HR MANAGER/ADMIN OPERATIONS
-- ============================================================

-- Update Application Status with History
DELIMITER //
CREATE PROCEDURE sp_update_app_status(
    IN p_application_id INT,
    IN p_new_status VARCHAR(30),
    IN p_remarks TEXT,
    IN p_changed_by VARCHAR(100)
)
BEGIN
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        ROLLBACK;
        RESIGNAL;
    END;
    
    START TRANSACTION;
    
    UPDATE applications
    SET status = p_new_status
    WHERE id = p_application_id;
    
    INSERT INTO application_status_history (application_id, status, remarks, changed_by)
    VALUES (p_application_id, p_new_status, p_remarks, p_changed_by);
    
    INSERT INTO audit_trail (user_type, action, table_name, record_id, user_action, details)
    VALUES ('HR Manager', 'Update', 'applications', p_application_id, p_changed_by, 
            CONCAT('Status changed to: ', p_new_status));
    
    COMMIT;
END //
DELIMITER ;

-- Record Hiring Decision
DELIMITER //
CREATE PROCEDURE sp_record_hiring_decision(
    IN p_application_id INT,
    IN p_decision VARCHAR(20),
    IN p_remarks TEXT,
    IN p_decided_by VARCHAR(150),
    IN p_start_date DATE
)
BEGIN
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        ROLLBACK;
        RESIGNAL;
    END;
    
    START TRANSACTION;
    
    INSERT INTO hiring_decisions (application_id, decision, remarks, decided_by, start_date)
    VALUES (p_application_id, p_decision, p_remarks, p_decided_by, p_start_date);
    
    UPDATE applications
    SET status = p_decision
    WHERE id = p_application_id;
    
    INSERT INTO application_status_history (application_id, status, remarks, changed_by)
    VALUES (p_application_id, p_decision, CONCAT('Hiring Decision: ', p_remarks), p_decided_by);
    
    INSERT INTO audit_trail (user_type, action, table_name, record_id, user_action, details)
    VALUES ('HR Manager', 'Approve', 'hiring_decisions', p_application_id, p_decided_by,
            CONCAT('Decision: ', p_decision, ' | Start Date: ', p_start_date));
    
    COMMIT;
END //
DELIMITER ;

-- Get Pending Decisions Count
DELIMITER //
CREATE PROCEDURE sp_get_pending_decisions_count(
    OUT p_count INT
)
BEGIN
    SELECT COUNT(*) INTO p_count
    FROM applications
    WHERE status = 'For Final Review';
END //
DELIMITER ;

-- Log Audit Trail
DELIMITER //
CREATE PROCEDURE sp_log_audit(
    IN p_user_type VARCHAR(30),
    IN p_username VARCHAR(150),
    IN p_action VARCHAR(200),
    IN p_table_name VARCHAR(100),
    IN p_record_id INT,
    IN p_details TEXT
)
BEGIN
    INSERT INTO audit_trail (user_type, username, action, table_name, record_id, details)
    VALUES (p_user_type, p_username, p_action, p_table_name, p_record_id, p_details);
END //
DELIMITER ;

-- ============================================================
--  FINAL VERIFICATION & TEST DATA
-- ============================================================

-- Verify all tables created
SELECT 'Database Setup Complete!' AS message;
SELECT 'Tables Created:' AS info;
SELECT COUNT(*) AS total_tables FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_SCHEMA = 'applicantsss';

-- Show all users
SELECT 'Test Users:' AS info;
SELECT username, full_name, email, role_id FROM hr_users ORDER BY role_id;

-- Show all job vacancies
SELECT 'Job Vacancies:' AS info;
SELECT id, title, department_id, status FROM job_vacancies;

-- Show all views
SELECT 'Database Views:' AS info;
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_SCHEMA = 'applicantsss' AND TABLE_TYPE = 'VIEW'
ORDER BY TABLE_NAME;