using HospitalDbClient.Data;

namespace HospitalDbClient.Reports;

public sealed class ReportDoctorsWorkloadForm : Form
{
    private readonly DataGridView _grid = new();

    public ReportDoctorsWorkloadForm()
    {
        Text = "Нагрузка врачей";
        Width = 1200;
        Height = 700;

        var btn = new Button
        {
            Text = "Сформировать",
            Dock = DockStyle.Top,
            Height = 40
        };

        btn.Click += (_, _) => LoadReport();

        _grid.Dock = DockStyle.Fill;
        _grid.AutoSizeColumnsMode =
            DataGridViewAutoSizeColumnsMode.Fill;

        Controls.Add(_grid);
        Controls.Add(btn);
    }

    private void LoadReport()
    {
        _grid.DataSource = Db.GetTable("""
            SELECT *
            FROM vw_doctors_high_workload
            ORDER BY total_patients DESC
        """);
    }
}