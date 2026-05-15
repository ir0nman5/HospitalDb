-- 1. Добавляем Больницы (ID будут 1 и 2)
INSERT INTO hospitals (name, inn, address) VALUES
('Городская клиническая больница №1', '123456789012', 'ул. Ленина, 10'),
('Центральная районная больница', '987654321098', 'ул. Пушкина, 15');

-- 2. Добавляем Должности (ID будут 1, 2 и 3)
INSERT INTO positions (name) VALUES
('Хирург'),
('Терапевт'),
('Главный врач');

-- 3. Добавляем Диагнозы (ID будут 1, 2 и 3)
INSERT INTO diagnoses (name, treatment_method) VALUES
('Острый аппендицит', 'Хирургическое вмешательство (аппендэктомия)'),
('Пневмония', 'Антибиотики, постельный режим, ингаляции'),
('Грипп', 'Симптоматическое лечение, противовирусные препараты');
	
-- 4. Добавляем Отделения
-- Обрати внимание: составной первичный ключ (hospital_id, department_id)
INSERT INTO departments (hospital_id, department_id, name, head_doctor) VALUES
(1, 1, 'Хирургическое отделение', 'Иванов И.И.'),
(1, 2, 'Терапевтическое отделение', 'Петров П.П.'),
(2, 1, 'Общее терапевтическое', 'Сидоров С.С.');

-- 5. Добавляем Врачей
INSERT INTO doctors (doctor_inn, hospital_id, department_id, full_name, position_id, hire_date) VALUES
('111111111111', 1, 1, 'Смирнов Алексей Викторович', 1, '2015-05-20'), -- Хирург в ГКБ №1
('222222222222', 1, 2, 'Кузнецова Мария Ивановна', 2, '2018-08-15'),  -- Терапевт в ГКБ №1
('333333333333', 2, 1, 'Попов Дмитрий Сергеевич', 2, '2020-02-10');    -- Терапевт в ЦРБ

-- 6. Добавляем Пациентов
-- Пациент 1: Лежит прямо сейчас (discharge_date = NULL)
INSERT INTO patients (hospital_id, department_id, doctor_inn, diagnosis_id, admission_date, diagnosis_date, full_name, patient_inn)
VALUES (1, 1, '111111111111', 1, '2023-11-01', '2023-11-01', 'Васильев Илья Романович', '444444444444');

-- Пациент 2: Уже выписан (есть discharge_date)
INSERT INTO patients (hospital_id, department_id, doctor_inn, diagnosis_id, admission_date, discharge_date, diagnosis_date, full_name, patient_inn, discharge_state)
VALUES (1, 2, '222222222222', 2, '2023-09-10', '2023-09-25', '2023-09-11', 'Николаева Анна Сергеевна', '555555555555', 'Выписан с улучшением');

-- Пациент 3: Лежит прямо сейчас в другой больнице
INSERT INTO patients (hospital_id, department_id, doctor_inn, diagnosis_id, admission_date, diagnosis_date, full_name, patient_inn)
VALUES (2, 1, '333333333333', 3, CURRENT_DATE - INTERVAL '3 days', CURRENT_DATE - INTERVAL '2 days', 'Федоров Петр Алексеевич', '666666666666');

	SELECT full_name, active_patients_count FROM doctors;


UPDATE doctors d
SET active_patients_count =
(
    SELECT COUNT(*)
    FROM patients p
    WHERE p.doctor_inn = d.doctor_inn
      AND p.discharge_date IS NULL
);

-- =========================================
-- ДОПОЛНИТЕЛЬНЫЕ БОЛЬНИЦЫ
-- =========================================

INSERT INTO hospitals (name, inn, address) VALUES
('Областная клиническая больница', '555666777888', 'пр. Мира, 22'),
('Медицинский центр "Здоровье"', '444333222111', 'ул. Советская, 7');

-- =========================================
-- ДОПОЛНИТЕЛЬНЫЕ ДОЛЖНОСТИ
-- =========================================

INSERT INTO positions (name) VALUES
('Кардиолог'),
('Невролог'),
('Реаниматолог'),
('Педиатр');

-- =========================================
-- ДОПОЛНИТЕЛЬНЫЕ ДИАГНОЗЫ
-- =========================================

INSERT INTO diagnoses (name, treatment_method) VALUES
('Инфаркт миокарда', 'Интенсивная терапия, кардиологическое наблюдение'),
('Мигрень', 'Обезболивающие препараты, наблюдение невролога'),
('Бронхит', 'Антибиотики, ингаляции, физиотерапия'),
('Сахарный диабет', 'Инсулинотерапия, диета, контроль сахара'),
('Гипертония', 'Антигипертензивная терапия');

-- =========================================
-- ДОПОЛНИТЕЛЬНЫЕ ОТДЕЛЕНИЯ
-- =========================================

INSERT INTO departments (hospital_id, department_id, name, head_doctor) VALUES
(3, 1, 'Кардиологическое отделение', 'Орлов А.А.'),
(3, 2, 'Неврологическое отделение', 'Егорова Е.Е.'),
(4, 1, 'Педиатрическое отделение', 'Морозова М.М.'),
(4, 2, 'Реанимация', 'Белов Б.Б.');

-- =========================================
-- ДОПОЛНИТЕЛЬНЫЕ ВРАЧИ
-- =========================================

INSERT INTO doctors
(doctor_inn, hospital_id, department_id, full_name, position_id, hire_date)
VALUES
('777777777777', 3, 1, 'Орлов Андрей Александрович', 4, '2012-03-15'),
('888888888888', 3, 2, 'Егорова Елена Евгеньевна', 5, '2017-11-02'),
('999999999999', 4, 1, 'Морозова Марина Михайловна', 7, '2019-06-20'),
('101010101010', 4, 2, 'Белов Борис Борисович', 6, '2010-01-12'),
('121212121212', 1, 2, 'Андреев Кирилл Павлович', 2, '2021-09-01');

-- =========================================
-- ДОПОЛНИТЕЛЬНЫЕ ПАЦИЕНТЫ
-- =========================================

INSERT INTO patients
(hospital_id, department_id, doctor_inn, diagnosis_id,
 admission_date, diagnosis_date,
 full_name, patient_inn)
VALUES
(3, 1, '777777777777', 4,
 CURRENT_DATE - INTERVAL '12 days',
 CURRENT_DATE - INTERVAL '11 days',
 'Громов Сергей Ильич',
 '131313131313');

INSERT INTO patients
(hospital_id, department_id, doctor_inn, diagnosis_id,
 admission_date, discharge_date, diagnosis_date,
 full_name, patient_inn, discharge_state)
VALUES
(3, 2, '888888888888', 5,
 CURRENT_DATE - INTERVAL '20 days',
 CURRENT_DATE - INTERVAL '10 days',
 CURRENT_DATE - INTERVAL '19 days',
 'Крылова Анна Викторовна',
 '141414141414',
 'Состояние улучшилось');

INSERT INTO patients
(hospital_id, department_id, doctor_inn, diagnosis_id,
 admission_date, diagnosis_date,
 full_name, patient_inn)
VALUES
(4, 1, '999999999999', 6,
 CURRENT_DATE - INTERVAL '5 days',
 CURRENT_DATE - INTERVAL '4 days',
 'Семенов Артем Олегович',
 '151515151515');

INSERT INTO patients
(hospital_id, department_id, doctor_inn, diagnosis_id,
 admission_date, diagnosis_date,
 full_name, patient_inn)
VALUES
(4, 2, '101010101010', 4,
 CURRENT_DATE - INTERVAL '2 days',
 CURRENT_DATE - INTERVAL '1 day',
 'Титов Николай Андреевич',
 '161616161616');

INSERT INTO patients
(hospital_id, department_id, doctor_inn, diagnosis_id,
 admission_date, discharge_date, diagnosis_date,
 full_name, patient_inn, discharge_state)
VALUES
(1, 2, '121212121212', 7,
 CURRENT_DATE - INTERVAL '15 days',
 CURRENT_DATE - INTERVAL '5 days',
 CURRENT_DATE - INTERVAL '14 days',
 'Зайцева Ирина Петровна',
 '171717171717',
 'Выписана в удовлетворительном состоянии');

INSERT INTO patients
(hospital_id, department_id, doctor_inn, diagnosis_id,
 admission_date, diagnosis_date,
 full_name, patient_inn)
VALUES
(1, 1, '111111111111', 1,
 CURRENT_DATE - INTERVAL '1 day',
 CURRENT_DATE - INTERVAL '1 day',
 'Баранов Максим Евгеньевич',
 '181818181818');

INSERT INTO patients
(hospital_id, department_id, doctor_inn, diagnosis_id,
 admission_date, diagnosis_date,
 full_name, patient_inn)
VALUES
(1, 1, '111111111111', 1,
 CURRENT_DATE - INTERVAL '7 days',
 CURRENT_DATE - INTERVAL '7 days',
 'Ефимов Даниил Сергеевич',
 '191919191919');

INSERT INTO patients
(hospital_id, department_id, doctor_inn, diagnosis_id,
 admission_date, discharge_date, diagnosis_date,
 full_name, patient_inn, discharge_state)
VALUES
(2, 1, '333333333333', 3,
 CURRENT_DATE - INTERVAL '14 days',
 CURRENT_DATE - INTERVAL '3 days',
 CURRENT_DATE - INTERVAL '13 days',
 'Киселева Наталья Олеговна',
 '202020202020',
 'Выписана без осложнений');


UPDATE doctors d
SET active_patients_count =
(
    SELECT COUNT(*)
    FROM patients p
    WHERE p.doctor_inn = d.doctor_inn
      AND p.discharge_date IS NULL
);