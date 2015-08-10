using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Example
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            // Убираем возможность повторного нажатия на кнопку
            //button1.IsEnabled = false;
            button1.Enabled = false;
            // Вызываем новую задачу, на этом выполнение функции закончится
            // а остаток функции установится в продолжение
            textBox1.Text = await new WebClient().DownloadStringTaskAsync("http://habrahabr.ru/");
            label1.Text = "Загрузка страницы завершена, начинается обработка";

            // В продолжении можно также запускать асинхронные операции со своим продолжением
            var result = await Task<string>.Factory.StartNew(() =>
            {
                Thread.Sleep(5000); // Имитация длительной обработки...
                return "Результат обработки";
            });
            // Продолжение второй асинхронной операции
            label1.Text = result;
            button1.Enabled = true;
        }
    }
}