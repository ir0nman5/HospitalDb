using HospitalDbClient.Data;
using Npgsql;

namespace HospitalDbClient.Reports;

public sealed class ReportPatientsByDepartmentForm : Form
{
    private readonly DataGridView _grid = new();

    public ReportPatientsByDepartmentForm()
    {
        Text = "Пациенты по отделениям";
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
        var table = Db.GetTable("""
            SELECT
                h.name AS hospital,
                d.name AS department,
                COUNT(*) AS patients_count
            FROM patients p
            JOIN hospitals h
                ON h.hospital_id = p.hospital_id
            JOIN departments d
                ON d.hospital_id = p.hospital_id
                AND d.department_id = p.department_id
            GROUP BY h.name, d.name
            HAVING COUNT(*) > 0
            ORDER BY patients_count DESC
        """);

        _grid.DataSource = table;
    }
}