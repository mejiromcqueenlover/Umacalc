package com.umacalc.android

import android.annotation.SuppressLint
import android.app.Notification
import android.app.NotificationChannel
import android.app.NotificationManager
import android.app.PendingIntent
import android.app.Service
import android.content.Context
import android.content.Intent
import android.content.SharedPreferences
import android.graphics.PixelFormat
import android.os.Build
import android.os.IBinder
import android.os.VibrationEffect
import android.os.Vibrator
import android.os.VibratorManager
import android.view.Gravity
import android.view.LayoutInflater
import android.view.MotionEvent
import android.view.View
import android.view.WindowManager
import android.widget.Button
import android.widget.ImageButton
import android.widget.LinearLayout
import android.widget.TextView
import androidx.core.app.NotificationCompat
import org.json.JSONObject

class FloatingHudService : Service() {

    private lateinit var windowManager: WindowManager
    private lateinit var hudView: View
    private lateinit var params: WindowManager.LayoutParams
    private lateinit var prefs: SharedPreferences

    private lateinit var txtTurnsRemaining: TextView
    private lateinit var hudStatsContainer: LinearLayout
    private lateinit var btnSpeed: Button
    private lateinit var btnStamina: Button
    private lateinit var btnPower: Button
    private lateinit var btnGuts: Button
    private lateinit var btnWisdom: Button

    private var speedTurns = 0
    private var staminaTurns = 0
    private var powerTurns = 0
    private var gutsTurns = 0
    private var wisdomTurns = 0
    private var totalRemainingTurns = 0
    private var isCollapsed = false

    companion object {
        const val EXTRA_HUD_DATA = "extra_hud_data"
        const val PREFS_NAME = "umacalc_state"
        const val NOTIFICATION_CHANNEL_ID = "umacalc_hud_channel"
        const val NOTIFICATION_ID = 4040
    }

    override fun onBind(intent: Intent?): IBinder? = null

    override fun onCreate() {
        super.onCreate()
        prefs = getSharedPreferences(PREFS_NAME, Context.MODE_PRIVATE)
        createNotificationChannel()
        startForeground(NOTIFICATION_ID, buildForegroundNotification())
        initFloatingOverlay()
    }

    override fun onStartCommand(intent: Intent?, flags: Int, startId: Int): Int {
        intent?.getStringExtra(EXTRA_HUD_DATA)?.let { data ->
            parseHudData(data)
            updateViewContent()
        }
        return START_NOT_STICKY
    }

    @SuppressLint("InflateParams", "ClickableViewAccessibility")
    private fun initFloatingOverlay() {
        windowManager = getSystemService(Context.WINDOW_SERVICE) as WindowManager

        val layoutFlag = if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
            WindowManager.LayoutParams.TYPE_APPLICATION_OVERLAY
        } else {
            @Suppress("DEPRECATION")
            WindowManager.LayoutParams.TYPE_PHONE
        }

        params = WindowManager.LayoutParams(
            WindowManager.LayoutParams.WRAP_CONTENT,
            WindowManager.LayoutParams.WRAP_CONTENT,
            layoutFlag,
            WindowManager.LayoutParams.FLAG_NOT_FOCUSABLE or WindowManager.LayoutParams.FLAG_LAYOUT_NO_LIMITS,
            PixelFormat.TRANSLUCENT
        ).apply {
            gravity = Gravity.TOP or Gravity.START
            x = 40
            y = 120
        }

        hudView = LayoutInflater.from(this).inflate(R.layout.layout_floating_hud, null)

        val dragBar = hudView.findViewById<LinearLayout>(R.id.hudDragBar)
        txtTurnsRemaining = hudView.findViewById(R.id.txtHudTurnsRemaining)
        hudStatsContainer = hudView.findViewById(R.id.hudStatsContainer)
        btnSpeed = hudView.findViewById(R.id.btnHudSpeed)
        btnStamina = hudView.findViewById(R.id.btnHudStamina)
        btnPower = hudView.findViewById(R.id.btnHudPower)
        btnGuts = hudView.findViewById(R.id.btnHudGuts)
        btnWisdom = hudView.findViewById(R.id.btnHudWisdom)

        val btnCollapse = hudView.findViewById<ImageButton>(R.id.btnCollapseHud)
        val btnClose = hudView.findViewById<ImageButton>(R.id.btnCloseHud)

        // Drag listener
        dragBar.setOnTouchListener(object : View.OnTouchListener {
            private var initialX = 0
            private var initialY = 0
            private var initialTouchX = 0f
            private var initialTouchY = 0f

            override fun onTouch(v: View?, event: MotionEvent): Boolean {
                when (event.action) {
                    MotionEvent.ACTION_DOWN -> {
                        initialX = params.x
                        initialY = params.y
                        initialTouchX = event.rawX
                        initialTouchY = event.rawY
                        return true
                    }
                    MotionEvent.ACTION_MOVE -> {
                        params.x = initialX + (event.rawX - initialTouchX).toInt()
                        params.y = initialY + (event.rawY - initialTouchY).toInt()
                        windowManager.updateViewLayout(hudView, params)
                        return true
                    }
                }
                return false
            }
        })

        // Stat Clickers
        btnSpeed.setOnClickListener { onStatClicked("Speed") }
        btnStamina.setOnClickListener { onStatClicked("Stamina") }
        btnPower.setOnClickListener { onStatClicked("Power") }
        btnGuts.setOnClickListener { onStatClicked("Guts") }
        btnWisdom.setOnClickListener { onStatClicked("Wisdom") }

        // Collapse / Expand
        btnCollapse.setOnClickListener {
            isCollapsed = !isCollapsed
            hudStatsContainer.visibility = if (isCollapsed) View.GONE else View.VISIBLE
            btnCollapse.setImageResource(if (isCollapsed) android.R.drawable.arrow_up_float else android.R.drawable.arrow_down_float)
        }

        // Close and reopen main activity
        btnClose.setOnClickListener {
            stopSelf()
            val appIntent = Intent(this, MainActivity::class.java).apply {
                addFlags(Intent.FLAG_ACTIVITY_NEW_TASK or Intent.FLAG_ACTIVITY_SINGLE_TOP)
            }
            startActivity(appIntent)
        }

        windowManager.addView(hudView, params)
    }

    private fun parseHudData(jsonString: String) {
        try {
            val json = JSONObject(jsonString)
            totalRemainingTurns = json.optInt("totalRemaining", 50)
            val stats = json.optJSONObject("stats")
            if (stats != null) {
                speedTurns = stats.optInt("Speed", 0)
                staminaTurns = stats.optInt("Stamina", 0)
                powerTurns = stats.optInt("Power", 0)
                gutsTurns = stats.optInt("Guts", 0)
                wisdomTurns = stats.optInt("Wisdom", 0)
            }
        } catch (_: Exception) {}
    }

    private fun updateViewContent() {
        txtTurnsRemaining.text = "$totalRemainingTurns Left"
        btnSpeed.text = "SPD\n$speedTurns"
        btnStamina.text = "STA\n$staminaTurns"
        btnPower.text = "POW\n$powerTurns"
        btnGuts.text = "GUT\n$gutsTurns"
        btnWisdom.text = "WIS\n$wisdomTurns"
    }

    private fun onStatClicked(statName: String) {
        vibrateBriefly()

        when (statName) {
            "Speed" -> if (speedTurns > 0) speedTurns--
            "Stamina" -> if (staminaTurns > 0) staminaTurns--
            "Power" -> if (powerTurns > 0) powerTurns--
            "Guts" -> if (gutsTurns > 0) gutsTurns--
            "Wisdom" -> if (wisdomTurns > 0) wisdomTurns--
        }

        if (totalRemainingTurns > 0) {
            totalRemainingTurns--
        }

        updateViewContent()
        saveStateToPreferences(statName)
    }

    private fun saveStateToPreferences(lastTrainedStat: String) {
        val json = JSONObject().apply {
            put("totalRemaining", totalRemainingTurns)
            put("lastTrained", lastTrainedStat)
            val stats = JSONObject().apply {
                put("Speed", speedTurns)
                put("Stamina", staminaTurns)
                put("Power", powerTurns)
                put("Guts", gutsTurns)
                put("Wisdom", wisdomTurns)
            }
            put("stats", stats)
        }

        prefs.edit().putString("latest_hud_state", json.toString()).apply()
    }

    private fun vibrateBriefly() {
        try {
            if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.S) {
                val vibratorManager = getSystemService(Context.VIBRATOR_MANAGER_SERVICE) as VibratorManager
                vibratorManager.defaultVibrator.vibrate(VibrationEffect.createOneShot(25, VibrationEffect.DEFAULT_AMPLITUDE))
            } else {
                @Suppress("DEPRECATION")
                val vibrator = getSystemService(Context.VIBRATOR_SERVICE) as Vibrator
                if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
                    vibrator.vibrate(VibrationEffect.createOneShot(25, VibrationEffect.DEFAULT_AMPLITUDE))
                } else {
                    vibrator.vibrate(25)
                }
            }
        } catch (_: Exception) {}
    }

    private fun createNotificationChannel() {
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
            val channel = NotificationChannel(
                NOTIFICATION_CHANNEL_ID,
                "Umacalc Floating HUD Service",
                NotificationManager.IMPORTANCE_LOW
            ).apply {
                description = "Keeps Umacalc floating HUD active over games"
            }
            val manager = getSystemService(NotificationManager::class.java)
            manager.createNotificationChannel(channel)
        }
    }

    private fun buildForegroundNotification(): Notification {
        val openIntent = Intent(this, MainActivity::class.java).apply {
            flags = Intent.FLAG_ACTIVITY_NEW_TASK or Intent.FLAG_ACTIVITY_SINGLE_TOP
        }
        val pendingIntent = PendingIntent.getActivity(
            this,
            0,
            openIntent,
            PendingIntent.FLAG_UPDATE_CURRENT or PendingIntent.FLAG_IMMUTABLE
        )

        return NotificationCompat.Builder(this, NOTIFICATION_CHANNEL_ID)
            .setContentTitle("Umacalc Floating HUD")
            .setContentText("Tap to return to Umacalc companion app")
            .setSmallIcon(android.R.drawable.ic_menu_compass)
            .setContentIntent(pendingIntent)
            .setOngoing(true)
            .build()
    }

    override fun onDestroy() {
        super.onDestroy()
        if (::windowManager.isInitialized && ::hudView.isInitialized) {
            windowManager.removeView(hudView)
        }
    }
}
