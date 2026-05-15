using HospitalDbClient.Data;

namespace HospitalDbClient.Forms;

public sealed class DiagnosesForm : CrudFormBase
{
    public DiagnosesForm() : base("Диагнозы") { }

    protected override string TableName => "diagnoses";
    protected override string KeyColumn => "diagnosis_id";
    protected override string[] SearchColumns => ["name", "treatment_method"];

    protected override List<FieldSpec> Fields => new()
    {
        new("diagnosis_id", "ID", FieldKind.Integer, IsRequired: false, IsPrimaryKey: true, IsIdentity: true),
        new("name", "Название"),
        new("treatment_method", "Метод лечения", FieldKind.Multiline, IsRequired: false)
    };
}