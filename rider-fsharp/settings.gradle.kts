pluginManagement {
    repositories {
        maven { setUrl("https://cache-redirector.jetbrains.com/plugins.gradle.org") }
    }
}

rootProject.name = "rider-fsharp"

include("protocol")
include("rider-fantomas")
