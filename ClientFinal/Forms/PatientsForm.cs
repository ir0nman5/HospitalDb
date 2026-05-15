using HospitalDbClient.Data;

namespace HospitalDbClient.Forms;

public sealed class PatientsForm : CrudFormBase
{
    public PatientsForm() : base("Пациенты") { }

    protected override string TableName => "patients";
    protected override string KeyColumn => "patient_id";
    protected override string[] SearchColumns => ["full_name", "patient_inn", "discharge_state"];

    protected override List<FieldSpec> Fields => new()
    {
        new("patient_id", "ID", FieldKind.Integer, IsRequired: false, IsPrimaryKey: true, IsIdentity: true),
        new("hospital_id", "ID больницы", FieldKind.Integer),
        new("department_id", "ID отделения", FieldKind.Integer),
        new("doctor_inn", "ИНН врача"),
        new("diagnosis_id", "ID диагноза", FieldKind.Integer),
        new("admission_date", "Дата поступления", FieldKind.Date),
        new("discharge_date", "Дата выписки", FieldKind.NullableDate, IsRequired: false),
        new("diagnosis_date", "Дата диагноза", FieldKind.Date),
        new("full_name", "ФИО пациента"),
        new("patient_inn", "ИНН пациента", FieldKind.Text, IsRequired: false),
        new("discharge_state", "Состояние при выписке", FieldKind.Multiline, IsRequired: false)
    };
}