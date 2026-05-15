using HospitalDbClient.Forms;
using HospitalDbClient.Views;
using HospitalDbClient.OneToMany;
using HospitalDbClient.Reports;

namespace HospitalDbClient.Forms;

public sealed class FormMain : Form
{
    public FormMain()
    {
        Text = "База данных больницы";
        Width = 1000;
        Height = 700;
        StartPosition = FormStartPosition.CenterScreen;

        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(20),
            AutoScroll = true
        };

        AddButton(panel, "Больницы", () => new HospitalsForm());
        AddButton(panel, "Отделения", () => new DepartmentsForm());
        AddButton(panel, "Должности", () => new PositionsForm());
        AddButton(panel, "Врачи", () => new DoctorsForm());
        AddButton(panel, "Диагнозы", () => new DiagnosesForm());
        AddButton(panel, "Пациенты", () => new PatientsForm());

        AddButton(panel,"VIEW: Активные пациенты",() => new ActivePatientsViewForm());

        AddButton(panel,"VIEW: Полная информация о пациентах",() => new PatientFullInfoViewForm());

        AddButton(panel, "Врач → Пациенты", () => new DoctorPatientsForm());

        AddButton(panel, "Отчёт: пациенты по отделениям",
            () => new ReportPatientsByDepartmentForm());

        AddButton(panel, "Отчёт: нагрузка врачей",
            () => new ReportDoctorsWorkloadForm());

        AddButton(panel, "Отчёт: поступления по диагнозам",
            () => new ReportAdmissionsByPeriodForm());

        Controls.Add(panel);
    }

    private void AddButton(FlowLayoutPanel panel, string text, Func<Form> creator)
    {
        var btn = new Button
        {
            Text = text,
            Width = 280,
            Height = 70,
            Margin = new Padding(10)
        };

        btn.Click += (_, _) =>
        {
            using var form = creator();
            form.ShowDialog(this);
        };

        panel.Controls.Add(btn);
    }
}