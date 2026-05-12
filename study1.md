# 登录
进入vr登录页面
首先输入用户名和账号
点击登录进入主页面
# 主页面
### UI部分
首先通过vr控制调出盘型选择器
选择一个盘型
点击确认进入不同的UI界面
初始默认选择第一个盘型
|盘型|UI界面|
|--|--|
|1|Binary Halo|
|2|Grande Halo|
|3|Probability Halo|
|4|Directional Peripheral Halo|
|5|Repeat Attempt Dashboard|
|6|Timeline Dashboard|
|7|Arousal Dashboard|
|Center|Next|
#### Binary Halo
大于70灯泡变亮
#### Grande Halo
分为多个等级，每个等级的颜色不同
颜色变换区间为0-40，40-60，60-70，70-80，80-90，90-100
#### Probability Halo
直接显示数值变化
#### Directional Peripheral Halo
#### Repeat Attempt Dashboard
Attemps是Speaking达到70的次数，RecentLevel和gradedHalo中的方块颜色一致
#### Timeline Dashboard
50-70，70-80，80-90，90-100分别为四个档次，低于50的情况则为黑色
#### Arousal Dashboard
左半部分和前面做的repeated dashboard一致，但是右边要加一个状态判断，判断依据是过去20s的speakingIntention的变化，如果增加了30以上为Active，如果减少了30以上为Negtive，如果变化少于30大于10为Calm，如果少于10为Stable
### 会议室部分
内有四个人偶，围绕桌子，有一个人偶是主控，其他人偶后面会加入新的控制功能，用于联机。
三个人偶循环进行一个表演脚本作为演示。
### 循环脚本
0-30s，DCY说话，ZJR和ZHZ在竞争
30-60s，DCY和ZJR说话，ZHZ上升
60-90s，三者沉默，都在变化