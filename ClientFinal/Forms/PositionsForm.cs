using HospitalDbClient.Data;

namespace HospitalDbClient.Forms;

public sealed class PositionsForm : CrudFormBase
{
    public PositionsForm() : base("Должности") { }

    protected override string TableName => "positions";
    protected override string KeyColumn => "position_id";
    protected override string[] SearchColumns => ["name"];

    protected override List<FieldSpec> Fields => new()
    {
        new("position_id", "ID", FieldKind.Integer, IsRequired: false, IsPrimaryKey: true, IsIdentity: true),
        new("name", "Название")
    };
}