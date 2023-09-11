using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace wpf1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private BindingList<Train> items = new();

        private Random rnd = new();
        private int secCnt = 0;
        // Action, на основании которой будем сохдавать задачу
        Action myAction;
        // переменные для перрывания задачи
        CancellationTokenSource cancelTokenSource;
        CancellationToken token;
        public MainWindow() {
            InitializeComponent();

            DateTime tmpd = DateTime.Now.Date;
            items = new BindingList<Train>() {
                new Train(1,  tmpd.AddHours(12), tmpd.AddHours(18)),
            };

            lvUsers.ItemsSource = items;

            Loaded += MainWindow_Loaded;
            startBtn.Click += StartBtn_Click;
            stopBtn.Click += StopBtn_Click;
            Closing += MainWindow_Closing;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e) {
            myAction = () =>
            {
                do {
                    if (secCnt % 3 == 0)
                        AddNewTrain();
                    if (secCnt % 5 == 0)
                        ModiftTrainList();


                    if (token != null)
                        token.ThrowIfCancellationRequested();

                    secCnt++;
                    Thread.Sleep(200);
                } while (true);
            };
        }

        private async void StartBtn_Click(object sender, RoutedEventArgs e) {
            stopBtn.IsEnabled = true;
            startBtn.IsEnabled = false;
            try {
                cancelTokenSource = new CancellationTokenSource();
                token = cancelTokenSource.Token;
                await Task.Run(myAction, token);
            }
            catch (OperationCanceledException ex) {

            }
        }

        private void StopBtn_Click(object sender, RoutedEventArgs e) {
            cancelTokenSource.Cancel();
            stopBtn.IsEnabled = false;
            startBtn.IsEnabled = true;
        }





        // Постарайтесь корректно отработать ситуацию, если после нажатия кнопки «Старт» пользователь пытается закрыть окно
        private void MainWindow_Closing(object? sender, CancelEventArgs e) {
            if (startBtn.IsEnabled)
                return;
            var dr = MessageBox.Show("Вы пытаетесь выйти из работающей программы. Вы уверены?", "Предупреждение", MessageBoxButton.YesNo);
            if (dr != MessageBoxResult.Yes)
                e.Cancel = true;
        }





        private void AddNewTrain() {
            Action actionAddNewTrain = () =>
            {
                // ID рейса каждый раз увеличивается на 1
                // Дата и время отправления выбираются случайным образом в интервале от текущего времени минус один час до текущее время плюс один час
                // Дата и время прибытия выбирается случайным образом в диапазоне от Даты отправления плюс один час до Дата отправления плюс десять часов
                var addMin1 = rnd.Next(-60, 60);
                var addMin2 = rnd.Next(60, 600);
                var newT = new Train(items.Count + 1, DateTime.Now.AddMinutes(addMin1), DateTime.Now.AddMinutes(addMin2));

                items.Add(newT);
            };

            if (!Dispatcher.CheckAccess()) // CheckAccess returns true if you're on the dispatcher thread
                Dispatcher.Invoke(actionAddNewTrain);
            else
                actionAddNewTrain();
        }

        // С интервалом в пять секунд тот же процесс проверяет статусы всех поездов
        // Если статус поезда «Запланирован», с вероятностью 30% оп переводится в статус «Отправлен»
        // Если статус поезда «Отправлен», с вероятностью в 20% поезд переходит в статус «Прибыл»
        private void ModiftTrainList() {
            Action modifyTrainList = () =>
            {
                foreach (var t in items) {
                    if (t.TrainStatus == TrainStatuses.Отправлен && rnd.Next(100) <= 20) 
                        t.TrainStatus = TrainStatuses.Прибыл;
                    if (t.TrainStatus == TrainStatuses.Запланирован && rnd.Next(100) <= 30) 
                        t.TrainStatus = TrainStatuses.Отправлен;
                }
            };

            if (!Dispatcher.CheckAccess()) // CheckAccess returns true if you're on the dispatcher thread
                Dispatcher.Invoke(modifyTrainList);
            else
                modifyTrainList();
        }

    }

    public class Train {
        
        public int Id { get; set; }
        public string Name { get { return $"Поезд {Id}"; } }
        public DateTime StartDt { get; set; }
        public DateTime FinishDt { get; set; }
        public TrainStatuses TrainStatus { get; set; }

        public Train() { }
        public Train(int id, DateTime startDt, DateTime finishDt) { 
            Id=id;
            StartDt=startDt;
            FinishDt=finishDt;
            TrainStatus = TrainStatuses.Запланирован; // Статус по умолчанию – «Запланирован»
        }


    }

    public enum TrainStatuses {
        Запланирован,
        Отправлен,
        Прибыл
    }


}
