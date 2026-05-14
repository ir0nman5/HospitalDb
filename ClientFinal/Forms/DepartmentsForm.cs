using HospitalDbClient.Data;

namespace HospitalDbClient.Forms;

public sealed class DepartmentsForm : CrudFormBase
{
    public DepartmentsForm() : base("Отделения") { }

    protected override string TableName => "departments";
    protected override string KeyColumn => "department_id";
    protected override string DefaultOrderBy => "hospital_id";
    protected override string[] SearchColumns => ["name", "head_doctor"];

    protected override List<FieldSpec> Fields => new()
    {
        new("hospital_id", "ID больницы", FieldKind.Integer),
        new("department_id", "ID отделения", FieldKind.Integer),
        new("name", "Название"),
        new("head_doctor", "Заведующий", FieldKind.Text, IsRequired: false)
    };
}