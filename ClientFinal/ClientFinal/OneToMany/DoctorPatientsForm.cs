using System.Data;
using Npgsql;
using HospitalDbClient.Data;

namespace HospitalDbClient.OneToMany;

public sealed class DoctorPatientsForm : Form
{
    private readonly ComboBox _cmbDoctors = new();
    private readonly DataGridView _grid = new();
    private readonly Button _btnAdd = new();
    private readonly Button _btnDelete = new();

    public DoctorPatientsForm()
    {
        Text = "Врач — пациенты";
        Width = 1200;
        Height = 800;
        StartPosition = FormStartPosition.CenterParent;

        var top = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 60, Padding = new Padding(8) };

        _cmbDoctors.Width = 350;
        _btnAdd.Text = "Добавить пациента";
        _btnDelete.Text = "Удалить пациента";

        _btnAdd.Click += (_, _) => AddPatient();
        _btnDelete.Click += (_, _) => DeletePatient();
        _cmbDoctors.SelectedIndexChanged += (_, _) => LoadPatients();

        top.Controls.Add(new Label { Text = "Врач:", AutoSize = true, Padding = new Padding(0, 8, 0, 0) });
        top.Controls.Add(_cmbDoctors);
        top.Controls.Add(_btnAdd);
        top.Controls.Add(_btnDelete);

        _grid.Dock = DockStyle.Fill;
        _grid.ReadOnly = true;
        _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _grid.MultiSelect = false;
        _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

        Controls.Add(_grid);
        Controls.Add(top);

        Shown += (_, _) => LoadDoctors();
    }

    private void LoadDoctors()
    {
        var table = Db.GetTable("""
            SELECT doctor_inn, full_name
            FROM doctors
            ORDER BY full_name
        """);

        _cmbDoctors.DisplayMember = "full_name";
        _cmbDoctors.ValueMember = "doctor_inn";
        _cmbDoctors.DataSource = table;
    }

    private void LoadPatients()
    {
        if (_cmbDoctors.SelectedValue is null) return;

        var doctorInn = _cmbDoctors.SelectedValue.ToString();

        var table = Db.GetTable("""
            SELECT patient_id, full_name, patient_inn, admission_date, discharge_date, diagnosis_date
            FROM patients
            WHERE doctor_inn = @doctor_inn
            ORDER BY admission_date DESC
        """, new NpgsqlParameter("doctor_inn", doctorInn));

        _grid.DataSource = table;
    }

    private void AddPatient()
    {
        if (_cmbDoctors.SelectedValue is null) return;

        var doctorInn = _cmbDoctors.SelectedValue.ToString();

        var fields = new List<FieldSpec>
        {
            new("hospital_id", "ID больницы", FieldKind.Integer),
            new("department_id", "ID отделения", FieldKind.Integer),
            new("doctor_inn", "ИНН врача", FieldKind.Text, IsPrimaryKey: true, ReadOnly: true),
            new("diagnosis_id", "ID диагноза", FieldKind.Integer),
            new("admission_date", "Дата поступления", FieldKind.Date),
            new("discharge_date", "Дата выписки", FieldKind.NullableDate, IsRequired: false),
            new("diagnosis_date", "Дата диагноза", FieldKind.Date),
            new("full_name", "ФИО пациента"),
            new("patient_inn", "ИНН пациента", FieldKind.Text, IsRequired: false),
            new("discharge_state", "Состояние при выписке", FieldKind.Multiline, IsRequired: false)
        };

        var initial = new Dictionary<string, object?>
        {
            ["doctor_inn"] = doctorInn
        };

        using var editor = new RecordEditorForm("Добавить пациента", fields, initial);
        if (editor.ShowDialog(this) == DialogResult.OK)
        {
            try
            {
                Db.Execute("""
                    INSERT INTO patients
                    (hospital_id, department_id, doctor_inn, diagnosis_id, admission_date, discharge_date, diagnosis_date, full_name, patient_inn, discharge_state)
                    VALUES
                    (@hospital_id, @department_id, @doctor_inn, @diagnosis_id, @admission_date, @discharge_date, @diagnosis_date, @full_name, @patient_inn, @discharge_state)
                """,
                new NpgsqlParameter("hospital_id", editor.Values["hospital_id"] ?? DBNull.Value),
                new NpgsqlParameter("department_id", editor.Values["department_id"] ?? DBNull.Value),
                new NpgsqlParameter("doctor_inn", editor.Values["doctor_inn"] ?? DBNull.Value),
                new NpgsqlParameter("diagnosis_id", editor.Values["diagnosis_id"] ?? DBNull.Value),
                new NpgsqlParameter("admission_date", editor.Values["admission_date"] ?? DBNull.Value),
                new NpgsqlParameter("discharge_date", editor.Values["discharge_date"] ?? DBNull.Value),
                new NpgsqlParameter("diagnosis_date", editor.Values["diagnosis_date"] ?? DBNull.Value),
                new NpgsqlParameter("full_name", editor.Values["full_name"] ?? DBNull.Value),
                new NpgsqlParameter("patient_inn", editor.Values["patient_inn"] ?? DBNull.Value),
                new NpgsqlParameter("discharge_state", editor.Values["discharge_state"] ?? DBNull.Value));

                LoadPatients();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private void DeletePatient()
    {
        if (_grid.CurrentRow?.Cells["patient_id"].Value is null)
            return;

        var id = _grid.CurrentRow.Cells["patient_id"].Value;

        if (MessageBox.Show("Удалить пациента?", "Подтверждение", MessageBoxButtons.YesNo) != DialogResult.Yes)
            return;

        Db.Execute("DELETE FROM patients WHERE patient_id = @id", new NpgsqlParameter("id", id));
        LoadPatients();
    }
}