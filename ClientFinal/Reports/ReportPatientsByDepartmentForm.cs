using HospitalDbClient.Data;
using System.Data;
using System.Linq;

namespace HospitalDbClient.Reports;

public sealed class ReportPatientsByDepartmentForm : Form
{
    private readonly DataGridView _grid = new();

    private readonly ComboBox _cmbSort = new();

    private readonly Label _lblTotal = new();

    public ReportPatientsByDepartmentForm()
    {
        Text = "Отчёт: пациенты по отделениям";

        Width = 1300;
        Height = 750;

        var top = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 60,
            Padding = new Padding(10)
        };

        _cmbSort.Width = 200;

        _cmbSort.Items.AddRange(new[]
        {
            "Количество пациентов",
            "Название отделения",
            "Название больницы"
        });

        _cmbSort.SelectedIndex = 0;

        var btn = new Button
        {
            Text = "Сформировать",
            Width = 150,
            Height = 35
        };

        btn.Click += (_, _) => LoadReport();

        top.Controls.Add(new Label
        {
            Text = "Сортировка:",
            AutoSize = true,
            Padding = new Padding(0, 8, 0, 0)
        });

        top.Controls.Add(_cmbSort);

        top.Controls.Add(btn);

        top.Controls.Add(_lblTotal);

        _grid.Dock = DockStyle.Fill;

        _grid.ReadOnly = true;

        _grid.AutoSizeColumnsMode =
            DataGridViewAutoSizeColumnsMode.Fill;

        Controls.Add(_grid);
        Controls.Add(top);
    }

    private void LoadReport()
    {
        string orderBy = _cmbSort.SelectedIndex switch
        {
            0 => "patients_count DESC",
            1 => "department_name ASC",
            _ => "hospital_name ASC"
        };

        var table = Db.GetTable($"""
            SELECT
                h.name AS hospital_name,

                d.name AS department_name,

                COUNT(p.patient_id) AS patients_count,

                COUNT(*) FILTER
                (
                    WHERE p.discharge_date IS NULL
                ) AS active_patients

            FROM patients p

            JOIN hospitals h
                ON h.hospital_id = p.hospital_id

            JOIN departments d
                ON d.hospital_id = p.hospital_id
               AND d.department_id = p.department_id

            GROUP BY
                h.name,
                d.name

            HAVING COUNT(p.patient_id) > 0

            ORDER BY {orderBy}
        """);

        _grid.DataSource = table;

        int total =
            table.AsEnumerable()
                .Sum(r => Convert.ToInt32(r["patients_count"]));

        _lblTotal.Text =
            $"Всего пациентов: {total}";
    }
}