using System;
using System.Windows.Forms;
using QuanLyTongHop.DAL;
using QuanLyTongHop.GUI;

namespace QuanLyTongHop
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            try
            {
                DbHelper.InitializeAllDatabases();
                Application.Run(new FormMain());
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khởi tạo hệ thống: " + ex.Message, "Lỗi CSDL", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}