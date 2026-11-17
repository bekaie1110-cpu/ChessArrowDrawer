
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Views;
using Android.Widget;

namespace ChessArrowDrawer;

[Service(
    Exported = false,
    ForegroundServiceType = ForegroundService.TypeSpecialUse)]
public class OverlayService : Service
{
    const int NotificationId = 1001;

    IWindowManager? manager;
    LinearLayout? sidebar;
    ArrowOverlayView? drawingView;

    WindowManagerLayoutParams? sidebarParams;
    WindowManagerLayoutParams? drawingParams;

    bool active;

    public override void OnCreate()
    {
        base.OnCreate();

        manager = GetSystemService(WindowService) as IWindowManager;

        CreateNotificationChannel();
        StartForeground(NotificationId, CreateNotification());

        CreateSidebar();
    }

    public override StartCommandResult OnStartCommand(
        Intent? intent,
        StartCommandFlags flags,
        int startId)
    {
        return StartCommandResult.Sticky;
    }

    void CreateNotificationChannel()
    {
        if (Build.VERSION.SdkInt < BuildVersionCodes.O)
            return;

        var channel = new NotificationChannel(
            "chess_overlay",
            "Chess Arrow Drawer",
            NotificationImportance.Low
        );

        var notificationManager =
            (NotificationManager?)GetSystemService(NotificationService);

        notificationManager?.CreateNotificationChannel(channel);
    }

    Notification CreateNotification()
    {
        return new Notification.Builder(this, "chess_overlay")
            .SetContentTitle("Chess Arrow Drawer")
            .SetContentText("Floating annotation service is running")
            .SetSmallIcon(Android.Resource.Drawable.IcMenuEdit)
            .SetOngoing(true)
            .Build();
    }

    WindowManagerLayoutParams CreateParams(
        int width,
        int height,
        GravityFlags gravity)
    {
        return new WindowManagerLayoutParams(
            width,
            height,
            WindowManagerTypes.ApplicationOverlay,
            WindowManagerFlags.NotFocusable |
            WindowManagerFlags.NotTouchModal |
            WindowManagerFlags.LayoutInScreen,
            Format.Translucent)
        {
            Gravity = gravity,
            X = 0,
            Y = 120
        };
    }

    void CreateSidebar()
    {
        sidebar = new LinearLayout(this)
        {
            Orientation = Orientation.Vertical
        };

        var background = new GradientDrawable();
        background.SetColor(Color.Rgb(20, 20, 25));
        background.SetCornerRadius(22);

        sidebar.Background = background;
        sidebar.SetPadding(8, 8, 8, 8);

        var arrowButton = new Button(this)
        {
            Text = "↗",
            TextSize = 25
        };

        var closeButton = new Button(this)
        {
            Text = "×",
            TextSize = 25
        };

        arrowButton.SetTextColor(Color.White);
        closeButton.SetTextColor(Color.Rgb(255, 70, 70));

        arrowButton.Click += (_, _) => ToggleDrawing();

        closeButton.Click += (_, _) =>
        {
            StopSelf();
        };

        sidebar.AddView(arrowButton, new LinearLayout.LayoutParams(
            65, 65));

        sidebar.AddView(closeButton, new LinearLayout.LayoutParams(
            65, 65));

        sidebarParams = CreateParams(
            WindowManagerLayoutParams.WrapContent,
            WindowManagerLayoutParams.WrapContent,
            GravityFlags.Top | GravityFlags.Start
        );

        manager?.AddView(sidebar, sidebarParams);
    }

    void ToggleDrawing()
    {
        if (manager == null)
            return;

        if (active)
        {
            DisableDrawing();
        }
        else
        {
            EnableDrawing();
        }
    }

    void EnableDrawing()
    {
        if (manager == null || active)
            return;

        drawingView = new ArrowOverlayView(this);

        drawingParams = CreateParams(
            WindowManagerLayoutParams.MatchParent,
            WindowManagerLayoutParams.MatchParent,
            GravityFlags.Top | GravityFlags.Start
        );

        drawingParams.Flags |= WindowManagerFlags.LayoutNoLimits;

        manager.AddView(drawingView, drawingParams);

        active = true;
    }

    void DisableDrawing()
    {
        if (manager == null)
            return;

        if (drawingView != null)
        {
            try
            {
                manager.RemoveView(drawingView);
            }
            catch
            {
                // View may already have been removed.
            }

            drawingView.Dispose();
            drawingView = null;
        }

        active = false;
    }

    public override void OnDestroy()
    {
        DisableDrawing();

        if (sidebar != null && manager != null)
        {
            try
            {
                manager.RemoveView(sidebar);
            }
            catch
            {
                // Already removed.
            }

            sidebar.Dispose();
            sidebar = null;
        }

        base.OnDestroy();
    }

    public override IBinder? OnBind(Intent? intent)
    {
        return null;
    }
}
