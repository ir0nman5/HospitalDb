using HospitalDbClient.Data;

namespace HospitalDbClient.Views;

public sealed class ActivePatientsViewForm : Form
{
    private readonly DataGridView _grid = new();

    public ActivePatientsViewForm()
    {
        Text = "Активные пациенты";
        Width = 1200;
        Height = 700;

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
                FROM vw_active_patients
                ORDER BY admission_date DESC
            """);
    }
}