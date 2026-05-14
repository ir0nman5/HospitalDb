namespace HospitalDbClient.Data;

public enum FieldKind
{
    Text,
    Integer,
    Date,
    NullableDate,
    Multiline
}

public sealed record FieldSpec(
    string Name,
    string Label,
    FieldKind Kind = FieldKind.Text,
    bool IsRequired = true,
    bool IsPrimaryKey = false,
    bool IsIdentity = false,
    bool ReadOnly = false
);