Localization
============
Localization has moved from Resource based localization to Database based localization,
so translation are no more in dll satellite libraries, but in table **LocalizationRecords**.
Look at `LocalizationDbContext <https://github.com/enkodellc/blazorboilerplate/blob/master/src/Server/BlazorBoilerplate.Storage/LocalizationDbContext.cs>`_.

The localization code is in `BlazorBoilerplate.Shared.Localizer project <https://github.com/enkodellc/blazorboilerplate/tree/master/src/Shared/BlazorBoilerplate.Shared.Localizer>`_.
The supported cultures are defined in `Settings.cs <https://github.com/enkodellc/blazorboilerplate/blob/master/src/Shared/BlazorBoilerplate.Shared.Localizer/Settings.cs>`_.

At this time **Data Annotations** do not support **IStringLocalizer<>**,
so to localize validation error messages, we have to use `Blazored.FluentValidation <https://github.com/Blazored/FluentValidation>`_.

Po files
________
This project adopts the `PO file format <https://www.gnu.org/software/gettext/manual/html_node/PO-Files.html>`_ as standard translations files.
In the admin section you can edit translations and import and export PO files.
There are a lot of free and paid tools and online services to manage these files. E.g.:

`Poedit <https://poedit.net/>`_

`Eazy Po <http://www.eazypo.ca/>`_

`Crowdin <https://www.crowdin.com/>`_ `(PO file format support) <https://support.crowdin.com/file-formats/po/>`_

To manage PO files BlazorBoilerplate uses `Karambolo.PO <https://github.com/adams85/po>`_ library.
So to handle plurals the Karambolo.PO syntax is used like in the example below.


Everywhere you need localized text, inject **IStringLocalizer<Global>** and look how it is used throughout the code.

::

 L["MsgId usually in english"]

 L["Email {0} is not valid", email]
 
 L["One item found", Plural.From("{0} items found", totalItemsCount)]
