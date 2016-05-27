using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 40704, "Strange duplication of listview headers when collapsing/expanding sections")]
	public class Bugzilla40704 : TestContentPage // or TestMasterDetailPage, etc ...
	{
		ListView listview;
		protected override void Init()
		{
			listview = new ListView(ListViewCachingStrategy.RecycleElement)
			{
				IsGroupingEnabled = true,
				HasUnevenRows = true,
				GroupHeaderTemplate = new DataTemplate(typeof(GroupHeaderViewCell)),
				ItemTemplate = new DataTemplate(typeof(ItemTestViewCell))
			};

			FillPatientsList();

			Content = listview;
		}

		private void FillPatientsList()
		{
			const int groupsNumber = 5;
			const int patientsNumber = 5;

			var patientGroups = new List<PatientsGroupViewModel>();
			for (var i = 0; i < groupsNumber; i++)
			{
				var patients = new List<PatientViewModel>();
				for (var j = 0; j < patientsNumber; j++)
				{
					var code = string.Format("{0}-{1}", i, j);
					patients.Add(new PatientViewModel(code));
				}

				patientGroups.Add(new PatientsGroupViewModel(patients)
				{
					Title = i.ToString()
				});
			}

			listview.ItemsSource = patientGroups;
		}

		class GroupHeaderViewCell : ViewCell
		{
			public GroupHeaderViewCell()
			{
				Height = 40;
				var grd = new Grid { BackgroundColor = Color.Aqua };
				var tapGesture = new TapGestureRecognizer();
				tapGesture.Tapped += HeaderCell_OnTapped;
				grd.GestureRecognizers.Add(tapGesture);
				var lbl = new Label { HorizontalOptions = LayoutOptions.FillAndExpand, TextColor = Color.Black, FontSize = 20 };
				lbl.SetBinding(Label.TextProperty, new Binding("Title"));
				grd.Children.Add(lbl);
				View = grd;
			}

			void HeaderCell_OnTapped(object sender, EventArgs e)
			{
				var cell = (Layout)sender;
				var vm = cell.BindingContext as PatientsGroupViewModel;

				if (vm != null)
				{
					vm.Toggle();
				}
			}
		}

		class ItemTestViewCell : ViewCell
		{
			public ItemTestViewCell()
			{
				Height = 50;
				var grd = new Grid { BackgroundColor = Color.Yellow };
				var lbl = new Label { HorizontalOptions = LayoutOptions.FillAndExpand, TextColor = Color.Black, FontSize = 16 };
				lbl.SetBinding(Label.TextProperty, new Binding("Code"));
				grd.Children.Add(lbl);
				View = grd;
			}
		}

		class RangeObservableCollection<T> : ObservableCollection<T>
		{
			private bool _suppressNotification = false;

			protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
			{
				if (!_suppressNotification)
					base.OnCollectionChanged(e);
			}

			public void AddRange(IEnumerable<T> list)
			{
				if (list == null)
					throw new ArgumentNullException("list");

				_suppressNotification = true;

				foreach (var item in list)
				{
					Add(item);
				}
				_suppressNotification = false;
				OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
			}
		}

		#region Helper Classes

		class PatientsGroupViewModel : RangeObservableCollection<PatientViewModel>
		{
			public bool IsCollapsed { get; private set; }
			public string Title { get; set; }

			private readonly List<PatientViewModel> _patients;

			public PatientsGroupViewModel(List<PatientViewModel> patients)
			{
				_patients = patients;

				UpdateCollection();
			}

			public void Toggle()
			{
				IsCollapsed = !IsCollapsed;

				UpdateCollection();
			}

			private void UpdateCollection()
			{
				if (!IsCollapsed)
				{
					AddRange(_patients);
				}
				else
				{
					Clear();
				}
			}
		}

		class PatientViewModel
		{
			public PatientViewModel(string code)
			{
				Code = code;
			}

			public string Code { get; set; }
		}

		#endregion

#if UITEST
		[Test]
		public void Issue1Test ()
		{
			RunningApp.Screenshot ("I am at Issue 1");
			RunningApp.WaitForElement (q => q.Marked ("IssuePageLabel"));
			RunningApp.Screenshot ("I see the Label");
		}
#endif
	}
}
