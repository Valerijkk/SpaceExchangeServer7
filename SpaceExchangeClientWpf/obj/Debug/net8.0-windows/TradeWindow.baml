   M S B A M L   `   `   `  ����  SpaceExchangeClientWpf(    "SpaceExchangeClientWpf.TradeWindow   5   	   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	     � Title$  Торговля��5   	   $	��300��6   $	��400��6'   .��5      � $��10q�6    � RowDefinitions 5   
   �� 5      $
��Auto �6   �� 5      �� 5	      $
��Auto �6   .+�5   
   ��  �� Orientation$ 
Horizontal=�6   .+�5      �� $��Ресурс:��6   $��Center=�6'   �� 5      -   5         ��Name$ TxtResource��5      $	��100��6+   $��5,0,0,0q�67   $��
QuantumOre��6H   �� 5      $��Количество:��6   $��10,0,0,0q�6+   $��Center=�6=   �� 5      -   5        $ 	TxtAmount��5      $��60��6)   $��5,0,0,0q�64   $��10��6E   5      �� 5   
   -   5        $ TxtTradeLog��5      $��1��6'   $��	0,10,0,10q�64    �� 
IsReadOnly$
 True��5       �� TextWrapping$
 Wrap=�6$   " �� VerticalScrollBarVisibility$
 Auto=�68   �� 5   
   $ 
Horizontal=�6   $��2��6.   $��Center=�6;   .+�5      �� -   5         ��Name$ BtnBuy��5      $��Купить��6%   $��80��66   $��5q�6A   6L   �� 5      -   5        $ BtnSell��5      $��Продать��6&   $��80��68   $��5q�6C   6N   5      5      5      