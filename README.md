#SummerFS
OS homework, a simple file system which can be mounted on your computer.

一个可以实际使用、挂载在真实物理机器上作为一个分区使用的文件系统

使用了 Dokany 内核驱动来加载该虚拟⽂文件系统（更多关于Dokany的信息: https://github.com/dokan-dev/dokany

为了运行该程序,需要安装.net framework 4.0。
支持 Windows 7 ~ Windows 10, x86/x64 操作系统。

#提醒
由于涉及到内核态驱动,因此可能会产生蓝屏(虽然测试下来⼏几乎不会蓝屏,
但是还是有很少很少概率会玩着玩着蓝屏)。因此,玩耍本项⽬前,请务必保存正在进行的其他⼯作！！！

#使⽤

1. 安装Dokany所需的VC++2013重分发库:http://www.microsoft.com/en-us/download/details.aspx?id=40784 (选择你操作系统的平台下载)

2. 安装Dokany:https://github.com/dokan-dev/dokany/releases/download/0.7.3-RC/DokanInstall_0.7.3-RC.exe

3. 打开该项⺫,编译运行(或直接打开bin目录下⽂件),选择⼀个⽂件系统 挂载盘符(如 M:\),点击创建。注意,请不要选择软盘符 A:\,这会导致挂载失败,原因并不清楚 = =

4. 如果没有弹出错误,则代表挂载成功。

5. 请打开您的资源管理器,访问您选择的挂载盘符号(⽐如M:\),随意玩耍。您可以创建文件、创建文件夹、粘贴其他地方文件进去、编辑文件,
  删除移动文件,修改文件属性。甚至您还可以直接双击打开里面文件或应⽤程序。

6. 如果您要关闭程序,请不要使用任务管理器结束进程,这可能导致蓝屏！！ 请点击「卸载」按钮来正常关闭程序。

#Q&A
1. 点击创建以后出现错误:Unable to load DLL “dokan.dll” 
下载 VS 2013 C++ 分发包:http://www.microsoft.com/en-us/download/details.aspx?id=40784 ,如果已安装 dokan,请在装完 VC++ 2013 分发包 以后重新安装 dokan,否则会有第三条错误。

2. 点击创建以后出现错误:Can’t install driver  右键,以管理员⾝身份运⾏行

3. 仍然Can’t install driver 
请先安装 VC++ 2013 分发包,再重新安装 dokan(先卸载再安装)
￼￼￼￼￼￼

#已验证可以正常工作的操作：

1. 创建⽂文件(含快捷方式) 

2. 创建文件夹

3. 读取文件(可以使用txt文件和记事本进⾏测试)

4. 写⼊文件

5. 重命名文件或⽂件夹 

6. 删除文件

7. 删除文件夹

8. 复制外部文件进去 

9. 将文件复制出去

#已知问题

由于时间关系,并没有做到非常完善,这个虚拟⽂件系统还很初步:

1. 由于未知原因,Windows10下复制进去的图⽚不能被预览.也不能被内置的图片应用打开,然而可以被画图板打开,也可以将文件复制出来以后再打开

2. 不支持⽂件锁定,不⽀持设定文件访问权限(ACL)

3. 不支持回收站

4. ⼤概还有各种其他小问题


