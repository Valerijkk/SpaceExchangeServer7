   M S B A M L   `   `   `  ����  SpaceExchangeClientWpf'    !SpaceExchangeClientWpf.MainWindow   5   	   W SPresentationCore, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	     � Title$  Space Exchange Client��5   	   $	��500��6'   $	��900��64   .��5      � $��10q�6    � RowDefinitions 5   
   �� 5      $
��Auto �6   �� 5      $��2* �6   �� 5	      $��2* �6   �� 5
      $
��Auto �6   .+�5   
   ��  �� Orientation$ 
Horizontal=�6   $��0,0,0,10q�6.   .+�5      �� -   5         ��Name$ TxtIp��5      $	��100��6%   $��	127.0.0.1��61   �� 5      -   5        $ TxtTcp��5      $��60��6&   $��5,0,0,0q�61   $
��5001��6B   �� 5      -   5        $ TxtUdp��5      $��60��6&   $��5,0,0,0q�61   $
��5002��6B   �� 5      -   5        $ TxtWs��5      $��60��6%   $��5,0,0,0q�60   $
��5003��6A   �� 5      -   5         ��Name$ BtnAddServer��5      $#��Добавить сервер��6+   $��5,0,0,0q�6E   6V   5      �� 5   
   -   5         ��Name$ ListServers��5      $��1��6'    �� DisplayMemberPath$ DisplayName��64   �� 5   
   -   5        $ 	TxtEvents��5      $��2��6%    �� 
IsReadOnly$
 True��62    �� TextWrapping$
 Wrap=�6D   "	 �� VerticalScrollBarVisibility$
	 Auto=�5      $
 �� HorizontalScrollBarVisibility$
 Disabled=�5      �� 5   
   $��3��6   $ 
Horizontal=�6"   $��0,10,0,0q�6;   .+�5       �� -   5        $ 
BtnConnect��5       $��Подключиться��6)   $	��120��6@   6L   �� 5!      -	   5        $ BtnRemoveServer��5!      $!��Удалить сервер��6.   $	��120��6G   $��5,0,0,0q�6S   6d   �� 5"      -
   5        $ BtnClearEvents��5"      $%��Очистить события��6-   $	��120��6H   $��5,0,0,0q�6T   6e   �� 5#      -   5        $ BtnOpenTrade��5#      $%��Открыть торговлю��6+   $	��120��6F   $��20,0,0,0q�6R   6d   5$      5%      5&      