using HospitalDbClient.Data;

namespace HospitalDbClient.Views;

public sealed class PatientFullInfoViewForm : Form
{
    private readonly DataGridView _grid = new();

    public PatientFullInfoViewForm()
    {
        Text = "Полная информация о пациентах";
        Width = 1400;
        Height = 800;

        _grid.Dock = DockStyle.Fill;

        _grid.ReadOnly = true;

        _grid.AutoSizeColumnsMode =
            DataGridViewAutoSizeColumnsMode.Fill;

        Controls.Add(_grid);

        Shown += (_, _) => LoadData();
    }

    private void LoadData()
    {
        _grid.DataSource =
            Db.GetTable("""
                SELECT *
                FROM vw_patient_full_info
                ORDER BY admission_date DESC
            """);
    }
}