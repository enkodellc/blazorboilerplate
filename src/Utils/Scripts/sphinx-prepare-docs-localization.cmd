cd ..\..\..\docs
sphinx-build -b gettext . _build/gettext
sphinx-intl update -p _build/gettext -l de_DE