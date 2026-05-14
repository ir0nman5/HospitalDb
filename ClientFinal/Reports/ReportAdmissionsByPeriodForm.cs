using HospitalDbClient.Data;

namespace HospitalDbClient.Reports;

public sealed class ReportAdmissionsByPeriodForm : Form
{
    private readonly DataGridView _grid = new();

    public ReportAdmissionsByPeriodForm()
    {
        Text = "Поступления по диагнозам";
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
            SELECT
                d.name,
                COUNT(*) AS admissions
            FROM patients p
            JOIN diagnoses d
                ON d.diagnosis_id = p.diagnosis_id
            GROUP BY d.name
            HAVING COUNT(*) > 0
            ORDER BY admissions DESC
        """);
    }
}