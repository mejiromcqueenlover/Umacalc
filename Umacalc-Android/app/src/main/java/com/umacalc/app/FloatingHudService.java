package com.umacalc.app;

import android.annotation.SuppressLint;
import android.app.Service;
import android.content.Context;
import android.content.Intent;
import android.content.SharedPreferences;
import android.graphics.Color;
import android.graphics.PixelFormat;
import android.os.Build;
import android.os.IBinder;
import android.os.VibrationEffect;
import android.os.Vibrator;
import android.os.VibratorManager;
import android.view.Gravity;
import android.view.MotionEvent;
import android.view.View;
import android.view.WindowManager;
import android.widget.Button;
import android.widget.LinearLayout;
import android.widget.TextView;
import org.json.JSONObject;

public class FloatingHudService extends Service {

    public static final String EXTRA_HUD_DATA = "extra_hud_data";
    private static final String PREFS_NAME = "umacalc_state";

    private WindowManager windowManager;
    private View hudView;
    private WindowManager.LayoutParams params;
    private SharedPreferences prefs;

    private TextView txtTurnsRemaining;
    private LinearLayout statsContainer;
    private Button btnSpeed;
    private Button btnStamina;
    private Button btnPower;
    private Button btnGuts;
    private Button btnWisdom;

    private int speedTurns = 0;
    private int staminaTurns = 0;
    private int powerTurns = 0;
    private int gutsTurns = 0;
    private int wisdomTurns = 0;
    private int totalRemainingTurns = 0;
    private boolean isCollapsed = false;

    @Override
    public IBinder onBind(Intent intent) {
        return null;
    }

    @Override
    public void onCreate() {
        super.onCreate();
        prefs = getSharedPreferences(PREFS_NAME, Context.MODE_PRIVATE);
        initFloatingOverlay();
    }

    @Override
    public int onStartCommand(Intent intent, int flags, int startId) {
        if (intent != null && intent.hasExtra(EXTRA_HUD_DATA)) {
            String data = intent.getStringExtra(EXTRA_HUD_DATA);
            parseHudData(data);
            updateViewContent();
        }
        return START_NOT_STICKY;
    }

    @SuppressLint("ClickableViewAccessibility")
    private void initFloatingOverlay() {
        windowManager = (WindowManager) getSystemService(Context.WINDOW_SERVICE);

        int layoutFlag;
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
            layoutFlag = WindowManager.LayoutParams.TYPE_APPLICATION_OVERLAY;
        } else {
            layoutFlag = WindowManager.LayoutParams.TYPE_PHONE;
        }

        params = new WindowManager.LayoutParams(
                WindowManager.LayoutParams.WRAP_CONTENT,
                WindowManager.LayoutParams.WRAP_CONTENT,
                layoutFlag,
                WindowManager.LayoutParams.FLAG_NOT_FOCUSABLE | WindowManager.LayoutParams.FLAG_LAYOUT_NO_LIMITS,
                PixelFormat.TRANSLUCENT
        );
        params.gravity = Gravity.TOP | Gravity.START;
        params.x = 40;
        params.y = 120;

        // Build view dynamically (Zero XML dependency for compilation simplicity)
        LinearLayout root = new LinearLayout(this);
        root.setOrientation(LinearLayout.VERTICAL);
        root.setBackgroundColor(0xDD111827);
        root.setPadding(dp(8), dp(8), dp(8), dp(8));

        // Drag Bar
        LinearLayout dragBar = new LinearLayout(this);
        dragBar.setOrientation(LinearLayout.HORIZONTAL);
        dragBar.setGravity(Gravity.CENTER_VERTICAL);
        dragBar.setPadding(0, 0, 0, dp(6));

        TextView title = new TextView(this);
        title.setText("UMACALC HUD");
        title.setTextSize(11);
        title.setTextColor(0xFF60A5FA);

        txtTurnsRemaining = new TextView(this);
        txtTurnsRemaining.setText("50 Left");
        txtTurnsRemaining.setTextSize(11);
        txtTurnsRemaining.setTextColor(0xFF34D399);
        txtTurnsRemaining.setPadding(dp(8), 0, dp(8), 0);

        Button btnCollapse = new Button(this);
        btnCollapse.setText("▲");
        btnCollapse.setTextSize(10);
        btnCollapse.setTextColor(Color.WHITE);
        btnCollapse.setBackgroundColor(0xFF1E293B);
        btnCollapse.setLayoutParams(new LinearLayout.LayoutParams(dp(36), dp(28)));

        Button btnClose = new Button(this);
        btnClose.setText("✕");
        btnClose.setTextSize(10);
        btnClose.setTextColor(0xFFFCA5A5);
        btnClose.setBackgroundColor(0xFF7F1D1D);
        LinearLayout.LayoutParams closeLp = new LinearLayout.LayoutParams(dp(36), dp(28));
        closeLp.setMargins(dp(4), 0, 0, 0);
        btnClose.setLayoutParams(closeLp);

        dragBar.addView(title);
        dragBar.addView(txtTurnsRemaining);
        dragBar.addView(btnCollapse);
        dragBar.addView(btnClose);

        // Stats Container
        statsContainer = new LinearLayout(this);
        statsContainer.setOrientation(LinearLayout.HORIZONTAL);

        btnSpeed = createStatButton("SPD", 0xFF2563EB);
        btnStamina = createStatButton("STA", 0xFFEA580C);
        btnPower = createStatButton("POW", 0xFFD97706);
        btnGuts = createStatButton("GUT", 0xFFDB2777);
        btnWisdom = createStatButton("WIS", 0xFF059669);

        statsContainer.addView(btnSpeed);
        statsContainer.addView(btnStamina);
        statsContainer.addView(btnPower);
        statsContainer.addView(btnGuts);
        statsContainer.addView(btnWisdom);

        root.addView(dragBar);
        root.addView(statsContainer);
        hudView = root;

        // Touch Dragging
        dragBar.setOnTouchListener(new View.OnTouchListener() {
            private int initialX;
            private int initialY;
            private float initialTouchX;
            private float initialTouchY;

            @Override
            public boolean onTouch(View v, MotionEvent event) {
                switch (event.getAction()) {
                    case MotionEvent.ACTION_DOWN:
                        initialX = params.x;
                        initialY = params.y;
                        initialTouchX = event.getRawX();
                        initialTouchY = event.getRawY();
                        return true;
                    case MotionEvent.ACTION_MOVE:
                        params.x = initialX + (int) (event.getRawX() - initialTouchX);
                        params.y = initialY + (int) (event.getRawY() - initialTouchY);
                        windowManager.updateViewLayout(hudView, params);
                        return true;
                }
                return false;
            }
        });

        // Click listeners
        btnSpeed.setOnClickListener(new View.OnClickListener() { @Override public void onClick(View v) { onStatClicked("Speed"); } });
        btnStamina.setOnClickListener(new View.OnClickListener() { @Override public void onClick(View v) { onStatClicked("Stamina"); } });
        btnPower.setOnClickListener(new View.OnClickListener() { @Override public void onClick(View v) { onStatClicked("Power"); } });
        btnGuts.setOnClickListener(new View.OnClickListener() { @Override public void onClick(View v) { onStatClicked("Guts"); } });
        btnWisdom.setOnClickListener(new View.OnClickListener() { @Override public void onClick(View v) { onStatClicked("Wisdom"); } });

        btnCollapse.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                isCollapsed = !isCollapsed;
                statsContainer.setVisibility(isCollapsed ? View.GONE : View.VISIBLE);
                btnCollapse.setText(isCollapsed ? "▼" : "▲");
            }
        });

        btnClose.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                stopSelf();
                Intent appIntent = new Intent(FloatingHudService.this, MainActivity.class);
                appIntent.addFlags(Intent.FLAG_ACTIVITY_NEW_TASK | Intent.FLAG_ACTIVITY_SINGLE_TOP);
                startActivity(appIntent);
            }
        });

        windowManager.addView(hudView, params);
    }

    private Button createStatButton(String label, int bgColor) {
        Button b = new Button(this);
        b.setText(label + "\n0");
        b.setTextSize(9);
        b.setTextColor(Color.WHITE);
        b.setBackgroundColor(bgColor);
        LinearLayout.LayoutParams lp = new LinearLayout.LayoutParams(dp(54), dp(44));
        lp.setMargins(dp(2), 0, dp(2), 0);
        b.setLayoutParams(lp);
        b.setPadding(0, 0, 0, 0);
        return b;
    }

    private int dp(int value) {
        return (int) (value * getResources().getDisplayMetrics().density);
    }

    private void parseHudData(String jsonString) {
        try {
            JSONObject json = new JSONObject(jsonString);
            totalRemainingTurns = json.optInt("totalRemaining", 50);
            JSONObject stats = json.optJSONObject("stats");
            if (stats != null) {
                speedTurns = stats.optInt("Speed", 0);
                staminaTurns = stats.optInt("Stamina", 0);
                powerTurns = stats.optInt("Power", 0);
                gutsTurns = stats.optInt("Guts", 0);
                wisdomTurns = stats.optInt("Wisdom", 0);
            }
        } catch (Exception ignored) {}
    }

    private void updateViewContent() {
        if (txtTurnsRemaining != null) txtTurnsRemaining.setText(totalRemainingTurns + " Left");
        if (btnSpeed != null) btnSpeed.setText("SPD\n" + speedTurns);
        if (btnStamina != null) btnStamina.setText("STA\n" + staminaTurns);
        if (btnPower != null) btnPower.setText("POW\n" + powerTurns);
        if (btnGuts != null) btnGuts.setText("GUT\n" + gutsTurns);
        if (btnWisdom != null) btnWisdom.setText("WIS\n" + wisdomTurns);
    }

    private void onStatClicked(String statName) {
        vibrateBriefly();

        if ("Speed".equals(statName) && speedTurns > 0) speedTurns--;
        else if ("Stamina".equals(statName) && staminaTurns > 0) staminaTurns--;
        else if ("Power".equals(statName) && powerTurns > 0) powerTurns--;
        else if ("Guts".equals(statName) && gutsTurns > 0) gutsTurns--;
        else if ("Wisdom".equals(statName) && wisdomTurns > 0) wisdomTurns--;

        if (totalRemainingTurns > 0) totalRemainingTurns--;

        updateViewContent();
    }

    private void vibrateBriefly() {
        try {
            if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.S) {
                VibratorManager vm = (VibratorManager) getSystemService(Context.VIBRATOR_MANAGER_SERVICE);
                if (vm != null) vm.getDefaultVibrator().vibrate(VibrationEffect.createOneShot(25, VibrationEffect.DEFAULT_AMPLITUDE));
            } else {
                Vibrator v = (Vibrator) getSystemService(Context.VIBRATOR_SERVICE);
                if (v != null) {
                    if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
                        v.vibrate(VibrationEffect.createOneShot(25, VibrationEffect.DEFAULT_AMPLITUDE));
                    } else {
                        v.vibrate(25);
                    }
                }
            }
        } catch (Exception ignored) {}
    }

    @Override
    public void onDestroy() {
        super.onDestroy();
        if (windowManager != null && hudView != null) {
            windowManager.removeView(hudView);
        }
    }
}
