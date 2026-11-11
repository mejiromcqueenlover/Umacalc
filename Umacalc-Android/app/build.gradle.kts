plugins {
    alias(libs.plugins.android.application)
}

android {
    namespace = "com.umacalc.app"
    compileSdk = 34

    defaultConfig {
        applicationId = "com.umacalc.app"
        minSdk = 21
        targetSdk = 33
        versionCode = 4
        versionName = "1.4"
    }

    signingConfigs {
        getByName("debug") {
            // standard debug key
        }
    }

    buildTypes {
        release {
            isMinifyEnabled = false
            signingConfig = signingConfigs.getByName("debug")
        }
        debug {
            isDebuggable = true
            signingConfig = signingConfigs.getByName("debug")
        }
    }

    compileOptions {
        sourceCompatibility = JavaVersion.VERSION_1_8
        targetCompatibility = JavaVersion.VERSION_1_8
    }

    lint {
        abortOnError = false
        checkReleaseBuilds = false
    }
}

dependencies {
    // Standard android framework
}
