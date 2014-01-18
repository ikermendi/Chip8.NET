using Chip8.NET.Chip8;
using Chip8.NET.Commands;
using Chip8.NET.Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Chip8.NET.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        public CPU CPU
        {
            get;
            private set;
        }

        private int cyclesPerSecond;
        private int opcodeCount;
        private long lastTick;
        private bool timerFinish;
        private int nextCycleCount;
        
        public Thread GameThread { get; set;}

        private bool IsPaused { get; set; }
        public bool CanStart { get; set; }
        public bool CanPause { get; set; }
        public bool CanInteract { get; set; }

        private List<Register> _registers;
        public List<Register> Registers
        {
            get
            {
                return _registers;
            }
            set
            {
                if (_registers == value)
                    return;
                _registers = value;
                RaisePropertyChanged("Registers");
            }
        }

        private string _pc;
        public string PC
        {
            get
            {
                return _pc;
            }
            set
            {
                if (_pc == value)
                    return;
                _pc = value;
                RaisePropertyChanged("PC");
            }
        }

        private string _i;
        public string I
        {
            get
            {
                return _i;
            }
            set
            {
                if (_i == value)
                    return;
                _i = value;
                RaisePropertyChanged("I");
            }
        }

        private string _dt;
        public string DT
        {
            get
            {
                return _dt;
            }
            set
            {
                if (_dt == value)
                    return;
                _dt = value;
                RaisePropertyChanged("DT");
            }
        }

        private string _st;
        public string ST
        {
            get
            {
                return _st;
            }
            set
            {
                if (_st == value)
                    return;
                _st = value;
                RaisePropertyChanged("ST");
            }
        }


        private List<InstructionWrapper> _instructions;
        public List<InstructionWrapper> Instructions
        {
            get
            {
                return _instructions;
            }
            set
            {
                if (_instructions == value)
                    return;
                _instructions = value;
                RaisePropertyChanged("Instructions");
            }
        }

        private int _currentInstructionIndex;
        public int CurrentInstructionIndex
        {
            get
            {
                return _currentInstructionIndex;
            }
            set
            {
                if (_currentInstructionIndex == value)
                    return;
                _currentInstructionIndex = value;
                RaisePropertyChanged("CurrentInstructionIndex");
            }
        }

        RelayCommand _startCommand;
        public ICommand StartCommand
        {
            get
            {
                if (_startCommand == null)
                {
                    _startCommand = new RelayCommand(param => this.Start(), param => this.CanStart);
                }
                return _startCommand;
            }
        }

        RelayCommand _pauseCommand;
        public ICommand PauseCommand
        {
            get
            {
                if (_pauseCommand == null)
                {
                    _pauseCommand = new RelayCommand(param => this.Pause(), param => this.CanPause);
                }
                return _pauseCommand;
            }
        }

        RelayCommand _nextCommand;
        public ICommand NextCommand
        {
            get
            {
                if (_nextCommand == null)
                {
                    _nextCommand = new RelayCommand(param => this.Next(), param => this.CanInteract);
                }
                return _nextCommand;
            }
        }

        RelayCommand _DTInterruptCommand;
        public ICommand DTInterruptCommand
        {
            get
            {
                if (_DTInterruptCommand == null)
                {
                    _DTInterruptCommand = new RelayCommand(param => this.DTInterrupt(), param => this.CanInteract);
                }
                return _DTInterruptCommand;
            }
        }


        public MainViewModel()
        {
            Messenger.Register(Notifications.FileSelectedNotification, (filename) =>
            {
                IsPaused = false;

                if (GameThread != null && GameThread.IsAlive)
                {
                    GameThread.Abort();
                }

                Configure();

                this.CanStart = true;
                CPU.LoadProgram(filename as string);
                Instructions = CPU.GetInstructions();
            });
        }

        private void Configure()
        {
            cyclesPerSecond = 10; // execute 600 opcodes per second
            opcodeCount = 0;
            nextCycleCount = 0;
            lastTick = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            CPU = new CPU();
        }

        private void Start()
        {
            this.CanStart = false;
            this.CanPause = true;

            GameThread = new Thread(() =>
            {
                while (true)
                {
                    if (!IsPaused)
                    {
                        Cycle();
                    }

                    //Decrease sound timer and delay timer every 1/60sec and redraw screen
                    if (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond - lastTick >= 1000 / 60)
                    {
                        if (IsPaused)
                        {
                            //If it is in pause mode every ~600 instructions decrease timers
                            if (nextCycleCount > 600)
                            {
                                nextCycleCount = 0;
                                timerFinish = CPU.DecreaseTimers();
                            }
                        }
                        else
                        {
                            CPU.DecreaseTimers();
                        }

                        lastTick = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
                        Messenger.Notify(Notifications.DrawNotification, null);
                        opcodeCount = 0;
                    }
                }
            });

            GameThread.Start();
        }


        private void Cycle()
        {
            Messenger.Notify(Notifications.HandleKeyNotification, null);

            if (opcodeCount < cyclesPerSecond)
            {
                this.CPU.Cycle();
                opcodeCount++;

                CurrentInstructionIndex = CPU.PC - 512;
                Registers = CPU.Registers.ToList<Register>();
                PC = string.Format("{0:X}", CPU.PC);
                I = string.Format("{0:X}", CPU.I);
                ST = Convert.ToString(CPU.SoundTimer);
                DT = Convert.ToString(CPU.DelayTimer);
            }
        }

        private void Pause()
        {
            if (IsPaused)
            {
                this.CanInteract = false;
                IsPaused = false;
            }
            else
            {
                this.CanInteract = true;
                IsPaused = true;
            }

            Messenger.Notify(Notifications.UpdatePauseNotification, IsPaused);
        }

        private void Next()
        {
            Cycle();
            nextCycleCount++;
        }

        private void DTInterrupt()
        {
            this.CanInteract = false;
            Task.Factory.StartNew(() =>
            {
                while (!timerFinish)
                {
                    Next();
                }

                this.CanInteract = true;
                timerFinish = false;
            });
        }
    }
}
