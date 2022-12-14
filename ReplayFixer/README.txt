Instructions to add new page
1. create page under views/pages (ui:UiPage) implement INavigableView<T>
2. create the viewmodel under ViewModels
3. add both page and viewmodel as a service in app.cs
4. add hidden navigationitem in container.xaml to override breadcrumb header
