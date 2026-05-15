using HospitalDbClient.Data;

namespace HospitalDbClient.Forms;

public sealed class DoctorsForm : CrudFormBase
{
    public DoctorsForm() : base("Врачи") { }

    protected override string TableName => "doctors";
    protected override string KeyColumn => "doctor_inn";
    protected override string[] SearchColumns => ["doctor_inn", "full_name"];

    protected override List<FieldSpec> Fields => new()
    {
        new("doctor_inn", "ИНН врача", FieldKind.Text, IsPrimaryKey: true),
        new("hospital_id", "ID больницы", FieldKind.Integer),
        new("department_id", "ID отделения", FieldKind.Integer),
        new("full_name", "ФИО"),
        new("position_id", "ID должности", FieldKind.Integer),
        new("hire_date", "Дата приёма", FieldKind.Date, IsRequired: false)
    };
}