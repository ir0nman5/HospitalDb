CREATE TABLE hospitals (
    hospital_id SERIAL PRIMARY KEY,
    name VARCHAR(200) NOT NULL,
    inn VARCHAR(12) UNIQUE NOT NULL,
    address TEXT NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE departments (
    hospital_id INT,
    department_id INT,
    name VARCHAR(200) NOT NULL,
    head_doctor VARCHAR(200),

    PRIMARY KEY (hospital_id, department_id),

    FOREIGN KEY (hospital_id)
        REFERENCES hospitals(hospital_id)
        ON DELETE CASCADE
);

CREATE TABLE positions (
    position_id SERIAL PRIMARY KEY,
    name VARCHAR(150) UNIQUE NOT NULL
);

CREATE TABLE doctors (
    doctor_inn VARCHAR(12) PRIMARY KEY,
    hospital_id INT NOT NULL,
    department_id INT NOT NULL,
    full_name VARCHAR(200) NOT NULL,
    position_id INT NOT NULL,
    hire_date DATE DEFAULT CURRENT_DATE,

    FOREIGN KEY (hospital_id, department_id)
        REFERENCES departments(hospital_id, department_id)
        ON DELETE CASCADE,

    FOREIGN KEY (position_id)
        REFERENCES positions(position_id)
);

CREATE TABLE diagnoses (
    diagnosis_id SERIAL PRIMARY KEY,
    name VARCHAR(200) UNIQUE NOT NULL,
    treatment_method TEXT
);

CREATE TABLE patients (
    patient_id SERIAL PRIMARY KEY,

    hospital_id INT NOT NULL,
    department_id INT NOT NULL,
    doctor_inn VARCHAR(12) NOT NULL,
    diagnosis_id INT NOT NULL,

    admission_date DATE NOT NULL,
    discharge_date DATE,

    diagnosis_date DATE NOT NULL,

    full_name VARCHAR(200) NOT NULL,
    patient_inn VARCHAR(12) UNIQUE,

    discharge_state TEXT,

    CHECK (discharge_date IS NULL OR discharge_date >= admission_date),

    FOREIGN KEY (hospital_id, department_id)
        REFERENCES departments(hospital_id, department_id),

    FOREIGN KEY (doctor_inn)
        REFERENCES doctors(doctor_inn),

    FOREIGN KEY (diagnosis_id)
        REFERENCES diagnoses(diagnosis_id)
);

CREATE INDEX idx_patients_admission_date ON patients(admission_date);

-- Индекс для быстрого поиска пациентов конкретного врача
CREATE INDEX idx_patients_doctor_inn ON patients(doctor_inn);

-- Индекс для поиска врачей по ФИО (полезно для клиентского приложения)
CREATE INDEX idx_doctors_full_name ON doctors(full_name);

--
CREATE OR REPLACE VIEW vw_active_patients AS
SELECT patient_id, full_name, patient_inn, admission_date, diagnosis_date
FROM patients
WHERE discharge_date IS NULL;

CREATE OR REPLACE VIEW vw_patient_full_info AS
SELECT 
    p.patient_id, 
    p.full_name AS patient_name,
    p.admission_date,
    p.discharge_date,
    h.name AS hospital_name,
    d.name AS department_name,
    doc.full_name AS doctor_name,
    diag.name AS diagnosis_name
FROM patients p
JOIN hospitals h ON p.hospital_id = h.hospital_id
JOIN departments d ON p.hospital_id = d.hospital_id AND p.department_id = d.department_id
JOIN doctors doc ON p.doctor_inn = doc.doctor_inn
JOIN diagnoses diag ON p.diagnosis_id = diag.diagnosis_id;


CREATE OR REPLACE VIEW vw_doctors_high_workload AS
SELECT 
    doc.full_name,
    COUNT(p.patient_id) AS total_patients,
    COUNT(*) FILTER (WHERE p.discharge_date IS NULL) AS active_patients
FROM doctors doc
LEFT JOIN patients p 
    ON doc.doctor_inn = p.doctor_inn
GROUP BY doc.full_name
ORDER BY total_patients DESC;

ALTER TABLE doctors 
ADD COLUMN active_patients_count INT DEFAULT 0;

CREATE OR REPLACE FUNCTION tf_update_active_patients_count()
RETURNS TRIGGER AS $$
BEGIN
    -- 1. Если добавили нового пациента и он не выписан
    IF TG_OP = 'INSERT' AND NEW.discharge_date IS NULL THEN
        UPDATE doctors 
        SET active_patients_count = active_patients_count + 1 
        WHERE doctor_inn = NEW.doctor_inn;
        
    -- 2. Если удалили пациента (и он не был выписан)
    ELSIF TG_OP = 'DELETE' AND OLD.discharge_date IS NULL THEN
        UPDATE doctors 
        SET active_patients_count = active_patients_count - 1 
        WHERE doctor_inn = OLD.doctor_inn;
        
    -- 3. Если обновили данные пациента
    ELSIF TG_OP = 'UPDATE' THEN
        -- Если пациента выписали (дата выписки изменилась с NULL на дату)
        IF NEW.discharge_date IS NOT NULL AND OLD.discharge_date IS NULL THEN
            UPDATE doctors 
            SET active_patients_count = active_patients_count - 1 
            WHERE doctor_inn = NEW.doctor_inn;
        -- Если выписку отменили (ошиблись)
        ELSIF NEW.discharge_date IS NULL AND OLD.discharge_date IS NOT NULL THEN
            UPDATE doctors 
            SET active_patients_count = active_patients_count + 1 
            WHERE doctor_inn = NEW.doctor_inn;
        -- Если невыписанного пациента передали другому врачу
        ELSIF NEW.doctor_inn != OLD.doctor_inn AND NEW.discharge_date IS NULL THEN
            UPDATE doctors SET active_patients_count = active_patients_count - 1 WHERE doctor_inn = OLD.doctor_inn;
            UPDATE doctors SET active_patients_count = active_patients_count + 1 WHERE doctor_inn = NEW.doctor_inn;
        END IF;
    END IF;
    
    RETURN NULL; -- Для AFTER триггеров возвращаем NULL
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_maintain_active_patients
AFTER INSERT OR UPDATE OR DELETE ON patients
FOR EACH ROW EXECUTE FUNCTION tf_update_active_patients_count();

