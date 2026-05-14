using HospitalDbClient.Data;

namespace HospitalDbClient.Forms;

public sealed class HospitalsForm : CrudFormBase
{
    public HospitalsForm() : base("Больницы") { }

    protected override string TableName => "hospitals";
    protected override string KeyColumn => "hospital_id";
    protected override string[] SearchColumns => ["name", "inn", "address"];

    protected override List<FieldSpec> Fields => new()
    {
        new("hospital_id", "ID", FieldKind.Integer, IsRequired: false, IsPrimaryKey: true, IsIdentity: true),
        new("name", "Название"),
        new("inn", "ИНН"),
        new("address", "Адрес", FieldKind.Multiline),
        new("created_at", "Дата создания", FieldKind.Date, IsRequired: false, ReadOnly: true)
    };
}