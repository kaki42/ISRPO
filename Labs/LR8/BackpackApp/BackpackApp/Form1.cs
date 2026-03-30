using BackpackApp.Debugging;
using BackpackApp.Models;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace BackpackApp
{
    public partial class Form1 : Form
    {
        private List<Item> _sourceItems;
        private List<Item> _bestSolution;

        public Form1()
        {
            InitializeComponent();
            SetupListView();
        }

        private void SetupListView()
        {
            listView1.View = View.Details;
            listView1.FullRowSelect = true;
            listView1.GridLines = true;
            listView1.Columns.Add("Название", 150);
            listView1.Columns.Add("Вес", 80);
            listView1.Columns.Add("Стоимость", 100);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            DatabaseInitializer.Initialize();
            DatabaseTester.TestConnection();
        }

        private void btnShowSource_Click(object sender, EventArgs e)
        {
            try
            {
                using (new ExecutionTimer("Загрузка исходных данных"))
                {
                    _sourceItems = DatabaseHelper.GetAllItems();
                }

                DisplayItems(_sourceItems);
                DebugLogger.LogItems(_sourceItems, "Исходные данные загружены");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                DebugLogger.Log($"Исключение: {ex}");
            }
        }

        private void btnSolve_Click(object sender, EventArgs e)
        {
            if (_sourceItems == null || _sourceItems.Count == 0)
            {
                MessageBox.Show("Сначала загрузите исходные данные.", "Предупреждение",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!int.TryParse(txtMaxWeight.Text, out int maxWeight) || maxWeight <= 0)
            {
                MessageBox.Show("Введите корректный положительный вес рюкзака.", "Ошибка ввода",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                using (new ExecutionTimer("Решение задачи о рюкзаке"))
                {
                    SolveKnapsack(maxWeight);
                }

                if (_bestSolution != null && _bestSolution.Count > 0)
                {
                    DisplayItems(_bestSolution);
                    DebugLogger.LogItems(_bestSolution, "Лучшее решение");
                }
                else
                {
                    listView1.Items.Clear();
                    MessageBox.Show("Не удалось подобрать набор предметов.", "Результат",
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при решении: {ex.Message}", "Ошибка",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                DebugLogger.Log($"Исключение в Solve: {ex}");
            }
        }

        private void SolveKnapsack(int maxWeight)
        {
            int n = _sourceItems.Count;
            int bestCost = 0;
            List<Item> bestCombination = new List<Item>();

            int totalCombinations = 1 << n;
            DebugLogger.Log($"Начинаем перебор {totalCombinations} комбинаций");

            for (int mask = 0; mask < totalCombinations; mask++)
            {
                int totalWeight = 0;
                int totalCost = 0;
                List<Item> current = new List<Item>();

                for (int i = 0; i < n; i++)
                {
                    if ((mask & (1 << i)) != 0)
                    {
                        totalWeight += _sourceItems[i].Weight;
                        totalCost += _sourceItems[i].Cost;
                        current.Add(_sourceItems[i]);
                    }
                }

                if (totalWeight <= maxWeight && totalCost > bestCost)
                {
                    bestCost = totalCost;
                    bestCombination = new List<Item>(current);
                    DebugLogger.Log($"Найдено улучшение: вес {totalWeight}, стоимость {totalCost}");
                }
            }

            _bestSolution = bestCombination;
            DebugLogger.Log($"Перебор завершён. Лучшая стоимость: {bestCost}");
        }

        private void DisplayItems(List<Item> items)
        {
            listView1.Items.Clear();
            foreach (var item in items)
            {
                var listItem = new ListViewItem(item.Name);
                listItem.SubItems.Add(item.Weight.ToString());
                listItem.SubItems.Add(item.Cost.ToString());
                listView1.Items.Add(listItem);
            }
        }
    }
}