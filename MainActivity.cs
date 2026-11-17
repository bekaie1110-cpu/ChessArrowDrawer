
using Android.App;
using Android.OS;
using Android.Content;
using Android.Provider;
using Android.Net;
using Android.Widget;
using Android;
using Android.Content.PM;

namespace ChessArrowDrawer;

[Activity(
    Label = "Chess Arrow Drawer",
    MainLauncher = true,
    Exported = true)]
public class MainActivity : Activity
{
    const int OverlayRequest = 100;

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        var layout = new LinearLayout(this)
        {
            Orientation = Orientation.Vertical
        };

        layout.SetPadding(35, 70, 35, 35);
        layout.SetBackgroundColor(Android.Graphics.Color.Rgb(12, 12, 16));

        var title = new TextView(this)
        {
            Text = "♟ CHESS ARROW DRAWER",
            TextSize = 25,
            Gravity = Android.Views.GravityFlags.Center
        };

        title.SetTextColor(Android.Graphics.Color.White);

        var description = new TextView(this)
        {
            Text = "Floating chess annotation tools",
            TextSize = 16,
            Gravity = Android.Views.GravityFlags.Center
        };

        description.SetTextColor(Android.Graphics.Color.LightGray);

        var start = new Button(this)
        {
            Text = "START FLOATING SIDEBAR"
        };

        start.Click += (_, _) => StartOverlay();

        var stop = new Button(this)
        {
            Text = "STOP OVERLAY"
        };

        stop.Click += (_, _) =>
        {
            StopService(new Intent(this, typeof(OverlayService)));
        };

        layout.AddView(title);
        layout.AddView(description);
        layout.AddView(start);
        layout.AddView(stop);

        SetContentView(layout);
    }

    void StartOverlay()
    {
        if (!Settings.CanDrawOverlays(this))
        {
            var intent = new Intent(
                Settings.ActionManageOverlayPermission,
                Uri.Parse("package:" + PackageName)
            );

            StartActivityForResult(intent, OverlayRequest);
            return;
        }

        if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu)
        {
            if (CheckSelfPermission(Manifest.Permission.PostNotifications)
                != Permission.Granted)
            {
                RequestPermissions(
                    new[] { Manifest.Permission.PostNotifications },
                    101
                );
            }
        }

        var service = new Intent(this, typeof(OverlayService));

        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            StartForegroundService(service);
        else
            StartService(service);
    }

    protected override void OnResume()
    {
        base.OnResume();

        if (Settings.CanDrawOverlays(this))
        {
            // Permission has been granted.
            // User can press START FLOATING SIDEBAR.
        }
    }
}
