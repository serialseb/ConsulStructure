if (-not (test-path "$env:APPVEYOR_BUILD_FOLDER/Gemfile")) {
    "gem 'chandler'" > "$env:APPVEYOR_BUILD_FOLDER/Gemfile"
}
bundle install --path vendor/ --binstubs