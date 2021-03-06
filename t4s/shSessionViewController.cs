using System;
using CoreGraphics;
using UIKit;
using Foundation;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace t4s
{
	public partial class shSessionViewController : UITableViewController
	{
		private showContestService todoService;

		public shSessionViewController (IntPtr handle) : base (handle)
		{
		}

		public override async void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			todoService = showContestService.DefaultService;
			await todoService.InitializeStoreAsync ();

			RefreshControl.ValueChanged += async (sender, e) => {
				await RefreshAsync ();
			};

			//await RefreshAsync ();
		}

		private async Task RefreshAsync ()
		{
            // start of RefreshAsync method
            if (todoService.User == null)
            {
                await showContestService.DefaultService.Authenticate(this);
                if (todoService.User == null)
                {
                    Console.WriteLine("couldn't login!!");
                    return;
                }
            }
            // rest of RefreshAsync method
            RefreshControl.BeginRefreshing ();
			await todoService.RefreshDataAsync ();
			RefreshControl.EndRefreshing ();

			TableView.ReloadData ();
		}

		public override void PrepareForSegue (UIStoryboardSegue segue, NSObject sender)
		{
			if (segue.Identifier == "TweetSegue") {
				var navctlr = segue.DestinationViewController as sessDViewController;
				if (navctlr != null) {
					var source = todoService.Items;
					var rowPath = TableView.IndexPathForSelectedRow;
					var item = todoService.Items [rowPath.Row];
					navctlr.SetTask(this, item);
				}
			}
		}

		#region UITableView methods
		public override nint RowsInSection (UITableView tableview, nint section)
		{
			if (todoService == null || todoService.Items == null)
				return 0;

			return todoService.Items.Count;
		}

		public override nint NumberOfSections (UITableView tableView)
		{
			return 1;
		}

		public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
		{
			const string CellIdentifier = @"Cell";
			var cell = tableView.DequeueReusableCell (CellIdentifier);
			if (cell == null) {
				cell = new UITableViewCell (UITableViewCellStyle.Default, CellIdentifier);
			}

			// Set the label on the cell and make sure the label color is black (in case this cell
			// has been reused and was previously greyed out
			var label = (UILabel)cell.ViewWithTag (1);
			label.TextColor = UIColor.Black;
			label.Text = todoService.Items [indexPath.Row].SessionTitle;

			return cell;
		}

		public override string TitleForDeleteConfirmation (UITableView tableView, NSIndexPath indexPath)
		{
			// Customize the Delete button to say "complete"
			return @"complete";
		}

		public override UITableViewCellEditingStyle EditingStyleForRow (UITableView tableView, NSIndexPath indexPath)
		{
			// Find the item that is about to be edited
			var item = todoService.Items [indexPath.Row];

			// If the item is complete, then this is just pending upload. Editing is not allowed
			if (item.ContestClosed)
				return UITableViewCellEditingStyle.None;

			// Otherwise, allow the delete button to appear
			return UITableViewCellEditingStyle.Delete;
		}

		public async override void CommitEditingStyle (UITableView tableView, UITableViewCellEditingStyle editingStyle, NSIndexPath indexPath)
		{
			// Find item that was commited for editing (completed)
			var item = todoService.Items [indexPath.Row];

			// Change the appearance to look greyed out until we remove the item
			var label = (UILabel)TableView.CellAt (indexPath).ViewWithTag (1);
			label.TextColor = UIColor.Gray;

			// Ask the todoService to set the item's complete value to YES, and remove the row if successful
			await todoService.CompleteItemAsync (item);

			// Remove the row from the UITableView
			tableView.DeleteRows (new [] { indexPath }, UITableViewRowAnimation.Top);
		}

		#endregion

		#region UI Actions


		#endregion

		[Export ("textFieldShouldReturn:")]
		public virtual bool ShouldReturn (UITextField textField)
		{
			textField.ResignFirstResponder ();
			return true;
		}

	}
}
