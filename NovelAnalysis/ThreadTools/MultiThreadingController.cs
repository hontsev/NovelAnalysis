using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Data;
using System.ComponentModel;
using System.Windows.Forms;

namespace NovelAnalysis
{
    public class MultiThreadingController
    {
        private bool isRun;
        private int nowWorkNumber;
        private int allWorkNumber;
        private const int checkStateTime = 500;
        private int threadNumber;
        private List<Thread> threads;
        private MyDelegate.sendIntDelegate workEvent;

        public MultiThreadingController(
            int workNum,
            MyDelegate.sendIntDelegate workCallback,
            int threadNum = 5
            )
        {
            threadNumber = threadNum;
            threads = new List<Thread>();
            workEvent = workCallback;
            isRun = false;
            nowWorkNumber = 0;
            allWorkNumber = workNum;
        }

        public MultiThreadingController(MyDelegate.sendIntDelegate workCallback)
            :this(0, workCallback,1)
        {
        }

        private int getNextWork()
        {
            if (nowWorkNumber < allWorkNumber)
            {
                return nowWorkNumber++;
            }
            else
            {
                return -1;
            }
        }

        private void work()
        {
            while (isRun)
            {
                int thisNumber = getNextWork();
                if (thisNumber == -1)
                {
                    break;
                }
                else
                {
                    workEvent(thisNumber);
                }
            }
        }

        /// <summary>
        /// 开始执行多线程任务。此函数会阻塞
        /// </summary>
        public void bStart()
        {
            isRun = true;
            for (int i = 0; i < threadNumber; i++)
            {
                Thread newThread = new Thread(work);
                threads.Add(newThread);
                newThread.Start();
            }
            while (true)
            {
                Thread.Sleep(checkStateTime);
                bool alive = false;
                foreach (var th in threads)
                {
                    if (th.ThreadState == ThreadState.Running)
                    {
                        alive = true;
                        break;
                    }
                }
                if (!alive)
                {
                    break;
                }
            }
        }
        
        /// <summary>
        /// 开始执行多线程任务。非阻塞
        /// </summary>
        public void Start()
        {
            new Thread(bStart).Start();
        }

        /// <summary>
        /// 中止任务。该函数会阻塞
        /// </summary>
        public void Stop()
        {
            isRun = false;
            while (true)
            {
                Thread.Sleep(checkStateTime);
                bool alive = false;
                foreach (var th in threads)
                {
                    if (th.ThreadState == ThreadState.Running)
                    {
                        alive = true;
                        break;
                    }
                }
                if (!alive)
                {
                    break;
                }
            }
        }

    }
}
