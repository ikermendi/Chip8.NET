using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chip8.NET.Chip8
{
	public class CPU
	{
		public Register[] Registers
		{
			get;
			private set;
		}

		Memory _memory;
		Graphic _graphic;
		Stack _stack;
		byte[] _key;
		ushort _currentOpcode;

		public byte DelayTimer
		{
			get;
			private set;
		}
		public byte SoundTimer
		{
			get;
			private set;
		}

		public ushort PC
		{
			get;
			private set;
		}
		public ushort I
		{
			get;
			private set;
		}

		public CPU()
		{
			//Init registers
			Registers = new Register[16];

			for (int i = 0; i < 16; i++)
			{
				Registers[i] = new Register("Register" + i);
			}

			//Init memory
			_memory = new Memory();
			
			//Init Stack
			_stack = new Stack();

			//Init graphic
			_graphic = new Graphic();

			//Init keys
			_key = new byte[16];
			for (int i = 0; i < _key.Length; i++)
			{
				_key[i] = 0;
			}

			//Init timers
			DelayTimer = 0;
			SoundTimer = 0;
		  
			PC = 0x200;        //Program counter starts at 0x200
			I = 0;             //Reset index register
			_currentOpcode = 0; //Reset current opcode
		}

		public void LoadProgram(string path)
		{
			byte[] buffer = System.IO.File.ReadAllBytes(path);

			if (buffer.Length > _memory.Size - 512)
			{

				throw new Exception("Program too big");
			}

			for (int i = 0; i < buffer.Length; i++)
			{
				_memory.SetData(512 + i, buffer[i]);
			}
		}


		public List<InstructionWrapper> GetInstructions()
		{
			List<InstructionWrapper> instructions = new List<InstructionWrapper>();
			byte[] memory = _memory.Data;
			for (int i = 0; i < memory.Length - 513; i++)
			{
				ushort opcode = (ushort)(_memory.Get((ushort)(512 + i)) << 8 | _memory.Get((ushort)(512 + i + 1)));
				string instruction = TranslateOpcode(opcode);
				if (instruction != null)
					instructions.Add(new InstructionWrapper(string.Format("{0:X} - {1:X}", (512 + i), (512 + i + 1)), instruction));
				else
					instructions.Add(new InstructionWrapper(string.Format("{0:X} - {1:X}", (512 + i), (512 + i + 1)), string.Format("DATA {0:X}", opcode)));
			}
			return instructions;
		}

		private string TranslateOpcode(ushort opcode)
		{
			string translatedOpcode = null;

			switch (opcode & 0xF000)
			{
				case 0x0000:
					switch (opcode & 0x00FF)
					{
						case 0x00E0: //00E0 - Clear the display
							translatedOpcode = "CLS"; 
							break;
						case 0x00EE: //00EE - Return from a subroutine
							translatedOpcode = "RET";
							break;
						default:
							break;
					}
					break;
				case 0x1000: //1nnn - Jump to location nnn
					translatedOpcode = string.Format("JP 0x{0:X}", opcode & 0x0FFF);
					break;
				case 0x2000: //2nnn - Calls subroutine at nnn
					translatedOpcode = string.Format("CALL 0x{0:X}", opcode & 0x0FFF);
					break;
				case 0x3000: //3xkk - Skip next instruction if Vx = kk
					translatedOpcode = string.Format("SE V{0:X}, 0x{1:X}", (opcode & 0x0F00) >> 8, opcode & 0x00FF);
					break;
				case 0x4000: //4xkk - Skip next instruction if Vx != kk
					translatedOpcode = string.Format("SNE V{0:X}, 0x{1:X}", (opcode & 0x0F00) >> 8, opcode & 0x00FF);
					break;
				case 0x5000: //5xy0 - Skip next instruction if Vx = Vy
					translatedOpcode = string.Format("SE V{0:X}, V{1:X}", (opcode & 0x0F00) >> 8, (opcode & 0x00F0) >> 4);
					break;
				case 0x6000: //6xkk - Set Vx = kk
					translatedOpcode = string.Format("LD V{0:X}, 0x{1:X}", (opcode & 0x0F00) >> 8, opcode & 0x00FF);
					break;
				case 0x7000: //7xkk - Set Vx = Vx + kk
					translatedOpcode = string.Format("ADD V{0:X}, 0x{1:X}", (opcode & 0x0F00) >> 8, opcode & 0x00FF);
					break;
				case 0x8000:
					{
						switch (opcode & 0x000F)
						{
							case 0x0000: //8xy0 - Set Vx = Vy
								translatedOpcode = string.Format("LD V{0:X}, V{1:X}", (opcode & 0x0F00) >> 8, (opcode & 0x00F0) >> 4);
								break;
							case 0x0001: //8xy1 - Set Vx = Vx OR Vy
								translatedOpcode = string.Format("OR V{0:X}, V{1:X}", (opcode & 0x0F00) >> 8, (opcode & 0x00F0) >> 4);
								break;
							case 0x0002: //8xy2 - Set Vx = Vx AND Vy
								translatedOpcode = string.Format("AND V{0:X}, V{1:X}", (opcode & 0x0F00) >> 8, (opcode & 0x00F0) >> 4);
								break;
							case 0x0003: //8xy3 - Set Vx = Vx XOR Vy
								translatedOpcode = string.Format("XOR V{0:X}, V{1:X}", (opcode & 0x0F00) >> 8, (opcode & 0x00F0) >> 4);
								break;
							case 0x0004: //8xy4 - Set Vx = Vx + Vy, set VF = carry
								translatedOpcode = string.Format("ADD V{0:X}, V{1:X}", (opcode & 0x0F00) >> 8, (opcode & 0x00F0) >> 4);
								break;
							case 0x0005: //8xy5 - Set Vx = Vx - Vy, set VF = NOT borrow
								translatedOpcode = string.Format("SUB V{0:X}, V{1:X}", (opcode & 0x0F00) >> 8, (opcode & 0x00F0) >> 4);
								break;
							case 0x0006: //8xy6 - Set Vx = Vx SHR 1
								translatedOpcode = string.Format("SHR V{0:X}", (opcode & 0x0F00) >> 8);
								break;
							case 0x0007: //8xy7 - Set Vx = Vy - Vx, set VF = NOT borrow
								translatedOpcode = string.Format("SUBN V{0:X}, V{1:X}", (opcode & 0x0F00) >> 8, (opcode & 0x00F0) >> 4);
								break;
							case 0x000E: //8xyE - Set Vx = Vx SHL 1
								translatedOpcode = string.Format("SHL  V{0:X}", (opcode & 0x0F00) >> 8);
								break;
							default:
								System.Console.WriteLine("Unknown opcode {0:X}", _currentOpcode);
								break;
						}
					}
					break;
				case 0x9000: //9xy0 - Skip next instruction if Vx != Vy
					translatedOpcode = string.Format("SNE V{0:X}, V{1:X}", (opcode & 0x0F00) >> 8, (opcode & 0x00F0) >> 4);
					break;
				case 0xA000: //Annn - Sets I to the address NNN.
					translatedOpcode = string.Format("LD I, {0:X}", (opcode & 0x0FFF));
					break;
				case 0xB000: //Bnnn - Jump to location nnn + V0
					translatedOpcode = string.Format("JP V0, {0:X}", (opcode & 0x0FFF));
					break;
				case 0xC000: //Cxkk - Set Vx = random byte AND kk
					translatedOpcode = string.Format("RND V{0:X}, {1:X}", (opcode & 0x0F00) >> 8, (opcode & 0x00FF));
					break;
				case 0xD000: // Dxyn: Draws a sprite at coordinate (VX, VY) that has a width of 8 pixels and a height of N pixels. 
					// Each row of 8 pixels is read as bit-coded starting from memory location I; 
					// I value doesn't change after the execution of this instruction. 
					// VF is set to 1 if any screen pixels are flipped from set to unset when the sprite is drawn, 
					// and to 0 if that doesn't happen
					translatedOpcode = string.Format("DRW V{0:X}, V{1:X}, {2:X}", (opcode & 0x0F00) >> 8, (opcode & 0x00F0) >> 4, (opcode & 0x000F));
					break;
				case 0xE000:
					{
						switch (opcode & 0x00FF)
						{
							case 0x009E: //Ex9E - Skip next instruction if key with the value of Vx is pressed
								translatedOpcode = string.Format("SKP V{0:X}", (opcode & 0x0F00) >> 8);
								break;
							case 0x00A1: //ExA1 - Skip next instruction if key with the value of Vx is not pressed
								translatedOpcode = string.Format("SKNP V{0:X}", (opcode & 0x0F00) >> 8);
								break;
							default:
								System.Console.WriteLine("Unknown opcode {0:X}", _currentOpcode);
								break;
						}
					}
					break;
				case 0xF000:
					{
						switch (opcode & 0x00FF)
						{
							case 0x0007: //Fx07 - Set Vx = delay timer value
								translatedOpcode = string.Format("LD V{0:X}, DT", (opcode & 0x0F00) >> 8);
								break;
							case 0x000A: //Fx0A - Wait for a key press, store the value of the key in Vx
								translatedOpcode = string.Format("LD V{0:X}, K", (opcode & 0x0F00) >> 8);
								break;
							case 0x0015: //Fx15 - Set delay timer = Vx
								translatedOpcode = string.Format("LD DT, V{0:X}", (opcode & 0x0F00) >> 8);
								break;
							case 0x0018: //Fx18 - Set sound timer = Vx
								translatedOpcode = string.Format("LD ST, V{0:X}", (opcode & 0x0F00) >> 8);
								break;
							case 0x001E: //Fx1E - Set I = I + Vx
								translatedOpcode = string.Format("ADD I, V{0:X}", (opcode & 0x0F00) >> 8);
								break;
							case 0x0029: // Fx29: Sets I to the location of the sprite for the character in VX. Characters 0-F (in hexadecimal) are represented by a 4x5 font
								translatedOpcode = string.Format("LD F, V{0:X}", (opcode & 0x0F00) >> 8);
								break;
							case 0x0033: //Fx33 - Store BCD representation of Vx in memory locations I, I+1, and I+2
								translatedOpcode = string.Format("LD B, V{0:X}", (opcode & 0x0F00) >> 8);
								break;
							case 0x0055: //Fx55 - Store registers V0 through Vx in memory starting at location I
								translatedOpcode = string.Format("LD [I], V{0:X}", (opcode & 0x0F00) >> 8);
								break;
							case 0x0065: //Fx65: Fills V0 to VX with values from memory starting at address I
								translatedOpcode = string.Format("LD V{0:X}, [I]", (opcode & 0x0F00) >> 8);
								break;
							default:
								break;
						}
					}
					break;
				default:
					break;
			}

			return translatedOpcode;
		}

		public void Cycle()
		{
			//Fetch opcode
			_currentOpcode = (ushort)(_memory.Get(PC) << 8 | _memory.Get((ushort)(PC + 1)));

			//Decode opcode
			switch (_currentOpcode & 0xF000)
			{
				case 0x0000:
					switch (_currentOpcode & 0x00FF)
					{
						case 0x00E0: //00E0 - Clear the display
							{
								_graphic.Clear();
								PC += (ushort) 2;
							}
							break;
						case 0x00EE: //00EE - Return from a subroutine
							{
								PC = _stack.Pop();
								PC += (ushort) 2;
							}
							break;
						default:
							System.Console.WriteLine("Unknown opcode {0:X}", _currentOpcode);
							break;
					}
					break;
				case 0x1000: //1nnn - Jump to location nnn
					{
						PC = (ushort)(_currentOpcode & 0x0FFF);
					}
					break;
				case 0x2000: //2nnn - Calls subroutine at nnn
					{
						_stack.Push(PC);
						PC = (ushort)(_currentOpcode & 0x0FFF);
					}
					break;
				case 0x3000: //3xkk - Skip next instruction if Vx = kk
					{
						Register vx = Registers[(_currentOpcode & 0x0F00) >> 8];
						if (vx.Reg == (_currentOpcode & 0x00FF))
							PC += 4;
						else
							PC += (ushort) 2;
					}
					break;
				case 0x4000: //4xkk - Skip next instruction if Vx != kk
					{
						Register vx = Registers[(_currentOpcode & 0x0F00) >> 8];
						if (vx.Reg != (_currentOpcode & 0x00FF))
							PC += 4;
						else
							PC += (ushort) 2;
					}
					break;
				case 0x5000: //5xy0 - Skip next instruction if Vx = Vy
					{
						Register vx = Registers[(_currentOpcode & 0x0F00) >> 8];
						Register vy = Registers[(_currentOpcode & 0x00F0) >> 4];
						if (vx.Reg == vy.Reg)
							PC += 4;
						else
							PC += (ushort) 2;
					}
					break;
				case 0x6000: //6xkk - Set Vx = kk
					{
						Register vx = Registers[(_currentOpcode & 0x0F00) >> 8];
						vx.Reg = (byte)(_currentOpcode & 0x00FF);
						PC += (ushort) 2;
					}
					break;
				case 0x7000: //7xkk - Set Vx = Vx + kk
					{
						Register vx = Registers[(_currentOpcode & 0x0F00) >> 8];
						vx.Reg += (byte)(_currentOpcode & 0x00FF);
						PC += (ushort) 2;
					}
					break;
				case 0x8000:
					{
						switch (_currentOpcode & 0x000F)
						{
							case 0x0000: //8xy0 - Set Vx = Vy
								{
									Register vx = Registers[(_currentOpcode & 0x0F00) >> 8];
									Register vy = Registers[(_currentOpcode & 0x00F0) >> 4];

									vx.Reg = vy.Reg;
									PC += (ushort) 2;
								}
								break;
							case 0x0001: //8xy1 - Set Vx = Vx OR Vy
								{
									Register vx = Registers[(_currentOpcode & 0x0F00) >> 8];
									Register vy = Registers[(_currentOpcode & 0x00F0) >> 4];

									vx.Reg |= vy.Reg;
									PC += (ushort) 2;
								}
								break;
							case 0x0002: //8xy2 - Set Vx = Vx AND Vy
								{
									Register vx = Registers[(_currentOpcode & 0x0F00) >> 8];
									Register vy = Registers[(_currentOpcode & 0x00F0) >> 4];

									vx.Reg &= vy.Reg;
									PC += (ushort) 2;
								}
								break;
							case 0x0003: //8xy3 - Set Vx = Vx XOR Vy
								{
									Register vx = Registers[(_currentOpcode & 0x0F00) >> 8];
									Register vy = Registers[(_currentOpcode & 0x00F0) >> 4];

									vx.Reg ^= vy.Reg;
									PC += (ushort) 2;
								}
								break;
							case 0x0004: //8xy4 - Set Vx = Vx + Vy, set VF = carry
								{
									Register vx = Registers[(_currentOpcode & 0x0F00) >> 8];
									Register vy = Registers[(_currentOpcode & 0x00F0) >> 4];

									if (vy.Reg > (0xFF - vx.Reg))
										Registers[0xF].Reg = 1;
									else
										Registers[0xF].Reg = 0;

									vx.Reg += vy.Reg;
									PC += (ushort) 2;
								}
								break;
							case 0x0005: //8xy5 - Set Vx = Vx - Vy, set VF = NOT borrow
								{
									Register vx = Registers[(_currentOpcode & 0x0F00) >> 8];
									Register vy = Registers[(_currentOpcode & 0x00F0) >> 4];
									Register vf = Registers[0xF];

									if (vy.Reg > vx.Reg)
										vf.Reg = 0; //there is a borrow
									else
										vf.Reg = 1;

									vx.Reg -= vy.Reg;
									PC += (ushort) 2;
								}
								break;
							case 0x0006: //8xy6 - Set Vx = Vx SHR 1
								{
									Register vx = Registers[(_currentOpcode & 0x0F00) >> 8];
									Register vf = Registers[0xF];

									vf.Reg = (byte)(vx.Reg & 0x01);
									vx.Reg >>= 1;
									PC += (ushort) 2;
								}
								break;
							case 0x0007: //8xy7 - Set Vx = Vy - Vx, set VF = NOT borrow
								{
									Register vx = Registers[(_currentOpcode & 0x0F00) >> 8];
									Register vy = Registers[(_currentOpcode & 0x00F0) >> 4];
									Register vf = Registers[0xF];

									if (vx.Reg > vy.Reg)
										vf.Reg = 0; //There is a borrow
									else
										vf.Reg = 1;

									vx.Reg = (byte)(vy.Reg - vx.Reg);
									PC += (ushort) 2;
								}
								break;
							case 0x000E: //8xyE - Set Vx = Vx SHL 1
								{
									Register vx = Registers[(_currentOpcode & 0x0F00) >> 8];
									Register vf = Registers[0xF];

									vf.Reg = (byte)(vx.Reg & 0x10);
									vx.Reg <<= 1;
									PC += (ushort) 2;
								}
								break;
							default:
								System.Console.WriteLine("Unknown opcode {0:X}", _currentOpcode);
								break;
						}
					}
					break;
				case 0x9000: //9xy0 - Skip next instruction if Vx != Vy
					{
						Register vx = Registers[(_currentOpcode & 0x0F00) >> 8];
						Register vy = Registers[(_currentOpcode & 0x00F0) >> 4];

						if (vx.Reg != vy.Reg)
							PC += 4;
						else
							PC += (ushort) 2;
					}
					break;
				case 0xA000: //Annn - Sets I to the address NNN.
					{
						I = (ushort)(_currentOpcode & 0x0FFF);
						PC += (ushort) 2;
					}
					break;
				case 0xB000: //Bnnn - Jump to location nnn + V0
					{
						Register v0 = Registers[0];
						ushort addr = (ushort)(_currentOpcode & 0x0FFF);
						PC = (ushort)(addr + v0.Reg);
					}
					break;
				case 0xC000: //Cxkk - Set Vx = random byte AND kk
					{
						Register vx = Registers[(_currentOpcode & 0x0F00) >> 8];
						byte kk = (byte)(_currentOpcode & 0x00FF);
						Random random = new Random();
						vx.Reg = (byte)(random.Next(0, 255) & kk);
						PC += (ushort) 2;
					}
					break;
				case 0xD000: // DXYN: Draws a sprite at coordinate (VX, VY) that has a width of 8 pixels and a height of N pixels. 
					// Each row of 8 pixels is read as bit-coded starting from memory location I; 
					// I value doesn't change after the execution of this instruction. 
					// VF is set to 1 if any screen pixels are flipped from set to unset when the sprite is drawn, 
					// and to 0 if that doesn't happen
					{
						Register vx = Registers[(_currentOpcode & 0x0F00) >> 8];
						Register vy = Registers[(_currentOpcode & 0x00F0) >> 4];
						Register vf = Registers[0xF];

						byte height = (byte)(_currentOpcode & 0x000F);
						byte pixel;

						vf.Reg = 0;
						for (int yLine = 0; yLine < height; yLine++)
						{
							pixel = _memory.Get((ushort)(I + yLine));
							for (int xLine = 0; xLine < 8; xLine++)
							{
								int position = vx.Reg + xLine + ((vy.Reg + yLine) * 64);
								if ((pixel & (0x80 >> xLine)) != 0)
								{
									if (_graphic.Get(position) == 1)
										vf.Reg = 1;

									_graphic.Draw(position);
								}
							}
						}
						PC += (ushort) 2;
					}
					break;
				case 0xE000:
					{
						switch (_currentOpcode & 0x00FF)
						{
							case 0x009E: //Ex9E - Skip next instruction if key with the value of Vx is pressed
								{
									Register vx = Registers[(_currentOpcode & 0x0F00) >> 8];
									if (_key[vx.Reg] != 0)
										PC += 4;
									else
										PC += (ushort) 2;
								}
								break;
							case 0x00A1: //ExA1 - Skip next instruction if key with the value of Vx is not pressed
								{
									Register vx = Registers[(_currentOpcode & 0x0F00) >> 8];
									if (_key[vx.Reg] == 0)
										PC += 4;
									else
										PC += (ushort) 2;
								}
								break;
							default:
								System.Console.WriteLine("Unknown opcode {0:X}", _currentOpcode);
								break;
						}
					}
					break;
				case 0xF000:
					{
						switch (_currentOpcode & 0x00FF)
						{
							case 0x0007: //Fx07 - Set Vx = delay timer value
								{
									Register vx = Registers[(_currentOpcode & 0x0F00) >> 8];
									vx.Reg = DelayTimer;
									PC += (ushort) 2;
								}
								break;
							case 0x000A: //Fx0A - Wait for a key press, store the value of the key in Vx
								{
									bool keyPressed = false;
									for (int i = 0; i < _key.Length; i++)
									{
										if(_key[i] == 1)
										{
											Register vx = Registers[(_currentOpcode & 0x0F00) >> 8];
											vx.Reg = (byte)i;
											keyPressed = true;
										}
									}

									if (!keyPressed)
										return;
									
									PC += (ushort) 2;
								}
								break;
							case 0x0015: //Fx15 - Set delay timer = Vx
								{
									Register vx = Registers[(_currentOpcode & 0x0F00) >> 8];
									DelayTimer = vx.Reg;
									PC += (ushort) 2;
								}
								break;
							case 0x0018: //Fx18 - Set sound timer = Vx
								{
									Register vx = Registers[(_currentOpcode & 0x0F00) >> 8];
									SoundTimer = vx.Reg;
									PC += (ushort) 2;
								}
								break;
							case 0x001E: //Fx1E - Set I = I + Vx
								{
									Register vx = Registers[(_currentOpcode & 0x0F00) >> 8];
									Register vf = Registers[0xF];

									if (I + vx.Reg > 0xFFF)	// VF is set to 1 when range overflow (I+VX>0xFFF), and 0 when there isn't.
										vf.Reg = 1;
									else
										vf.Reg = 0;

									I += vx.Reg;
									PC += (ushort) 2;
								}
								break;
							case 0x0029: // Fx29: Sets I to the location of the sprite for the character in VX. Characters 0-F (in hexadecimal) are represented by a 4x5 font
								{
									Register vx = Registers[(_currentOpcode & 0x0F00) >> 8];
									I = (ushort)(vx.Reg * 0x5);
									PC += (ushort) 2;
								}
								break;
							case 0x0033: //Fx33 - Store BCD representation of Vx in memory locations I, I+1, and I+2
								{
									Register vx = Registers[(_currentOpcode & 0x0F00) >> 8];
									
									_memory.SetData(I, (byte)(vx.Reg / 100));
									_memory.SetData(I + 1, (byte)((vx.Reg / 10) % 10));
									_memory.SetData(I + 1, (byte)((vx.Reg % 100) % 10));
									PC += (ushort) 2;
								}
								break;
							case 0x0055: //Fx55 - Store registers V0 through Vx in memory starting at location I
								{
									Register v0 = Registers[0x0];

									for (int i = 0; i < ((_currentOpcode & 0x0F00) >> 8); i++)
									{
										Register v = Registers[i];
										_memory.SetData(I + i, v.Reg);
									}

									// On the original interpreter, when the operation is done, I = I + X + 1.
									I += (ushort)(((_currentOpcode & 0x0F00) >> 8) + 1);

									PC += (ushort) 2;
								}
								break;
							case 0x0065: //Fx65: Fills V0 to VX with values from memory starting at address I
								{
									Register v0 = Registers[0x0];

									for (int i = 0; i <= ((_currentOpcode & 0x0F00) >> 8); i++)
									{
										Register v = Registers[i];
										v.Reg = _memory.Get((ushort)(I + i));
									}

									// On the original interpreter, when the operation is done, I = I + X + 1.
									I += (ushort)(((_currentOpcode & 0x0F00) >> 8) + 1);

									PC += (ushort) 2;
								}
								break;
							default:
								System.Console.WriteLine("Unknown opcode {0:X}", _currentOpcode);
								break;
						}
					}
					break;
				default:
					System.Console.WriteLine("Unknown opcode {0:X}", _currentOpcode);
					break;
			}
		}

		public byte[] GetDisplay()
		{
			return _graphic.GetScreen();
		}

		public void InputKey(int keyIndex, bool pressed)
		{
			_key[keyIndex] = (byte)(pressed ? 1 : 0);
		}

		public bool DecreaseTimers()
		{
			//Update timers
			if (DelayTimer > 0)
				DelayTimer--;

			if (SoundTimer > 0)
			{
				if (SoundTimer == 1)
					System.Console.WriteLine("BEEP\n");
				SoundTimer--;
			}

			if (DelayTimer == 0)
				return true;

			return false;
		}
	}
}
