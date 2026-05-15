using System.Globalization;

namespace HospitalDbClient.Data;

public sealed class RecordEditorForm : Form
{
    private readonly List<FieldSpec> _fields;
    private readonly Dictionary<string, Control> _controls = new();

    public Dictionary<string, object?> Values { get; private set; } = new();

    public RecordEditorForm(string title, IEnumerable<FieldSpec> fields, Dictionary<string, object?>? initialValues = null)
    {
        _fields = fields.ToList();
        Text = title;
        Width = 700;
        Height = 700;
        StartPosition = FormStartPosition.CenterParent;

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = _fields.Count + 1,
            AutoScroll = true,
            Padding = new Padding(12)
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 200));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        for (int i = 0; i < _fields.Count; i++)
        {
            var field = _fields[i];

            var label = new Label
            {
                Text = field.Label,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };

            Control input = field.Kind switch
            {
                FieldKind.Multiline => new TextBox
                {
                    Multiline = true,
                    Height = 80,
                    Dock = DockStyle.Fill,
                    ScrollBars = ScrollBars.Vertical
                },
                FieldKind.Date or FieldKind.NullableDate => new DateTimePicker
                {
                    Dock = DockStyle.Fill,
                    Format = DateTimePickerFormat.Short,
                    ShowCheckBox = field.Kind == FieldKind.NullableDate,
                    Checked = field.Kind != FieldKind.NullableDate
                },
                _ => new TextBox { Dock = DockStyle.Fill }
            };

            if (field.ReadOnly || field.IsIdentity)
                input.Enabled = false;

            if (initialValues != null && initialValues.TryGetValue(field.Name, out var value) && value != null)
            {
                switch (input)
                {
                    case TextBox tb:
                        tb.Text = value.ToString();
                        break;
                    case DateTimePicker dtp:
                        if (DateTime.TryParse(value.ToString(), out var dt))
                        {
                            dtp.Value = dt;
                            dtp.Checked = true;
                        }
                        break;
                }
            }

            _controls[field.Name] = input;

            layout.Controls.Add(label, 0, i);
            layout.Controls.Add(input, 1, i);
        }

        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            Height = 45
        };

        var btnSave = new Button { Text = "Сохранить", Width = 120 };
        var btnCancel = new Button { Text = "Отмена", Width = 120 };

        btnSave.Click += (_, _) =>
        {
            try
            {
                Values = CollectValues();
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        };

        btnCancel.Click += (_, _) =>
        {
            DialogResult = DialogResult.Cancel;
            Close();
        };

        buttons.Controls.Add(btnSave);
        buttons.Controls.Add(btnCancel);

        layout.Controls.Add(buttons, 0, _fields.Count);
        layout.SetColumnSpan(buttons, 2);

        Controls.Add(layout);
    }

    private Dictionary<string, object?> CollectValues()
    {
        var result = new Dictionary<string, object?>();

        foreach (var field in _fields)
        {
            var control = _controls[field.Name];

            object? value = control switch
            {
                TextBox tb when field.Kind == FieldKind.Integer =>
                    string.IsNullOrWhiteSpace(tb.Text) ? null : int.Parse(tb.Text, CultureInfo.InvariantCulture),

                TextBox tb when field.Kind is FieldKind.Text or FieldKind.Multiline =>
                    string.IsNullOrWhiteSpace(tb.Text) ? null : tb.Text,

                DateTimePicker dtp when field.Kind == FieldKind.Date =>
                    dtp.Value.Date,

                DateTimePicker dtp when field.Kind == FieldKind.NullableDate =>
                    dtp.Checked ? dtp.Value.Date : null,

                _ => null
            };

            if (field.IsRequired && value is null && !field.IsIdentity)
                throw new InvalidOperationException($"Поле '{field.Label}' обязательно.");

            result[field.Name] = value;
        }

        return result;
    }
}