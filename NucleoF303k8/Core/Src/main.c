/* USER CODE BEGIN Header */
/**
  ******************************************************************************
  * @file           : main.c
  * @brief          : Main program body
  ******************************************************************************
  * @attention
  *
  * Copyright (c) 2026 STMicroelectronics.
  * All rights reserved.
  *
  * This software is licensed under terms that can be found in the LICENSE file
  * in the root directory of this software component.
  * If no LICENSE file comes with this software, it is provided AS-IS.
  *
  ******************************************************************************
  */
/* USER CODE END Header */
/* Includes ------------------------------------------------------------------*/
#include "main.h"

/* Private includes ----------------------------------------------------------*/
/* USER CODE BEGIN Includes */
#include "stdbool.h"
/* USER CODE END Includes */

/* Private typedef -----------------------------------------------------------*/
/* USER CODE BEGIN PTD */
CAN_TxHeaderTypeDef TxHeader;
/* USER CODE END PTD */

/* Private define ------------------------------------------------------------*/
/* USER CODE BEGIN PD */

/* USER CODE END PD */

/* Private macro -------------------------------------------------------------*/
/* USER CODE BEGIN PM */

/* USER CODE END PM */

/* Private variables ---------------------------------------------------------*/
CAN_HandleTypeDef hcan;

UART_HandleTypeDef huart2;

/* USER CODE BEGIN PV */
uint32_t TxMailbox;
uint8_t TxData[8];
uint32_t CanRxIde;
uint32_t CanRxId;
uint32_t CanRxDlc;
uint8_t  CanRxData[8] = {0,1,2,3,4,5,6,7};
bool CanRxFlag = false;

/*PCから来る*/
uint8_t RxChar;
char RxBuffer[32];
int RxIndex=0;
bool CanTxFlag = false;
uint32_t CanTxIde;
uint32_t CanTxId;
uint32_t CanTxDlc;
uint8_t  CanTxData[8] = {0,1,2,3,4,5,6,7};

int cnt;
/* USER CODE END PV */

/* Private function prototypes -----------------------------------------------*/
void SystemClock_Config(void);
static void MX_GPIO_Init(void);
static void MX_USART2_UART_Init(void);
static void MX_CAN_Init(void);
/* USER CODE BEGIN PFP */
void HAL_CAN_RxFifo0MsgPendingCallback(CAN_HandleTypeDef *hcan)
{
    CAN_RxHeaderTypeDef RxHeader;
    uint8_t RxData[8];
    if (HAL_CAN_GetRxMessage(hcan, CAN_RX_FIFO0, &RxHeader, RxData) == HAL_OK)
    {
    	CanRxIde = RxHeader.IDE;
        CanRxId = (RxHeader.IDE == CAN_ID_STD)? RxHeader.StdId : RxHeader.ExtId;     // ID
        CanRxDlc = RxHeader.DLC;                                   // DLC
        CanRxData[0] = RxData[0];                                                // Data
        CanRxData[1] = RxData[1];
        CanRxData[2] = RxData[2];
        CanRxData[3] = RxData[3];
        CanRxData[4] = RxData[4];
        CanRxData[5] = RxData[5];
        CanRxData[6] = RxData[6];
        CanRxData[7] = RxData[7];
        CanRxFlag = true;
    }
}

void HAL_UART_RxCpltCallback(UART_HandleTypeDef *huart){
	if(huart->Instance==USART2){
		if(RxChar == '\r'){
			RxBuffer[RxIndex]='\0';
			CanTxFlag = true;
			RxIndex = 0;
		}else{
			if(RxIndex < 31){
				RxBuffer[RxIndex] = RxChar;
				RxIndex++;
			}
		}
		HAL_UART_Receive_IT(&huart2, &RxChar,1);
	}
}

uint32_t parseHex(char* str, int len){
	uint32_t val = 0;
	for(int i = 0;i<len;i++){
		char c = str[i];
		uint8_t v = 0;
		if(c >= '0' && c<= '9') v = c - '0';//数字に変換
		else if(c >= 'A' && c <= 'F') v = c - 'A' + 10;//大文字の16進数も10進数に変換
		else if(c >= 'a' && c <= 'f') v = c - 'a' + 10;//小文字の16進数も10進数に変換
		val = (val << 4) | v;
	}
	return val;
}
/* USER CODE END PFP */

/* Private user code ---------------------------------------------------------*/
/* USER CODE BEGIN 0 */

/* USER CODE END 0 */

/**
  * @brief  The application entry point.
  * @retval int
  */
int main(void)
{

  /* USER CODE BEGIN 1 */

  /* USER CODE END 1 */

  /* MCU Configuration--------------------------------------------------------*/

  /* Reset of all peripherals, Initializes the Flash interface and the Systick. */
  HAL_Init();

  /* USER CODE BEGIN Init */

  /* USER CODE END Init */

  /* Configure the system clock */
  SystemClock_Config();

  /* USER CODE BEGIN SysInit */

  /* USER CODE END SysInit */

  /* Initialize all configured peripherals */
  MX_GPIO_Init();
  MX_USART2_UART_Init();
  MX_CAN_Init();
  /* USER CODE BEGIN 2 */
  CAN_FilterTypeDef filter;
  uint32_t fId   = 0x0000;        // フィルターID
  uint32_t fMask = 0x0000; // フィルターマスク

  filter.FilterIdHigh         = fId >> 16;             // フィルターIDの上位16ビット
  filter.FilterIdLow          = fId;                   // フィルターIDの下位16ビット
  filter.FilterMaskIdHigh     = fMask >> 16;           // フィルターマスクの上位16ビット
  filter.FilterMaskIdLow      = fMask;                 // フィルターマスクの下位16ビット
  filter.FilterScale          = CAN_FILTERSCALE_32BIT; // 32モード
  filter.FilterFIFOAssignment = CAN_FILTER_FIFO0;      // FIFO0へ格納
  filter.FilterBank           = 0;
  filter.FilterMode           = CAN_FILTERMODE_IDMASK; // IDマスクモード
  filter.SlaveStartFilterBank = 14;
  filter.FilterActivation     = ENABLE;

  HAL_CAN_ConfigFilter(&hcan, &filter);
  HAL_CAN_Start(&hcan);
  HAL_CAN_ActivateNotification(&hcan, CAN_IT_RX_FIFO0_MSG_PENDING);
  HAL_UART_Receive_IT(&huart2, &RxChar, 1);
  /* USER CODE END 2 */

  /* Infinite loop */
  /* USER CODE BEGIN WHILE */
  while (1)
  {
//	  if(0 < HAL_CAN_GetTxMailboxesFreeLevel(&hcan)){//もしメールボックスに空きがあれば
//	      TxHeader.StdId = 0x750;                 // CAN ID
//	      TxHeader.RTR = CAN_RTR_DATA;            // フレームタイプはデータフレーム
//	      TxHeader.IDE = CAN_ID_STD;              // 標準ID(11ﾋﾞｯﾄ)
//	      TxHeader.DLC = 8;                       // データ長は8バイトに
//	      TxHeader.TransmitGlobalTime = DISABLE;  // ???
//	      TxData[0] = 0x11;//送信データを格納
//	      TxData[1] = 0x22;
//	      TxData[2] = 0x33;
//	      TxData[3] = 0x44;
//	      TxData[4] = 0x55;
//	      TxData[5] = 0x66;
//	      TxData[6] = 0x77;
//	      TxData[7] = 0x88;
//	      HAL_CAN_AddTxMessage(&hcan, &TxHeader, TxData, &TxMailbox);//TxDataをTxHeaderの設定通りに送信する
//	  }
	  if(CanRxFlag){
		  char txBuffer[32];
		  int index = 0;
		  if(CanRxIde==CAN_ID_STD){
			  index += sprintf(&txBuffer[index], "t%03lX%lu", CanRxId, CanRxDlc);
		  }else{
			  index += sprintf(&txBuffer[index], "T%08lX%lu", CanRxId, CanRxDlc);
		  }
		  for(int i = 0;i<CanRxDlc;i++){
			  index += sprintf(&txBuffer[index], "%02X", CanRxData[i]);
		  }
		  txBuffer[index++] = '\r';
		  txBuffer[index] = '\0';

		  HAL_UART_Transmit(&huart2, (uint8_t*)txBuffer, index, 100);
		  CanRxFlag=false;
	  }

	  if(CanTxFlag){
		  if(RxBuffer[0]=='t'){
			  TxHeader.IDE = CAN_ID_STD;
			  TxHeader.StdId = parseHex(&RxBuffer[1],3);
			  TxHeader.DLC = parseHex(&RxBuffer[4],1);
			  for(int i = 0; i < TxHeader.DLC;i++){
				  TxData[i] = (uint8_t)parseHex(&RxBuffer[5 + (i * 2)], 2);
			  }
		  }else if(RxBuffer[0] == 'T'){
			  TxHeader.IDE = CAN_ID_EXT;
			  TxHeader.ExtId = parseHex(&RxBuffer[1],8);
			  TxHeader.DLC = parseHex(&RxBuffer[9],1);
			  for(int i = 0; i < TxHeader.DLC;i++){
				  TxData[i] = (uint8_t)parseHex(&RxBuffer[10 + (i * 2)], 2);
			  }
		  }
		  TxHeader.RTR = CAN_RTR_DATA;
		  TxHeader.TransmitGlobalTime = DISABLE;
		  if(0 < HAL_CAN_GetTxMailboxesFreeLevel(&hcan)){
			  HAL_CAN_AddTxMessage(&hcan, &TxHeader, TxData, &TxMailbox);
		  }
		  CanTxFlag = false;
	  }
	  HAL_Delay(1);
    /* USER CODE END WHILE */

    /* USER CODE BEGIN 3 */
  }
  /* USER CODE END 3 */
}

/**
  * @brief System Clock Configuration
  * @retval None
  */
void SystemClock_Config(void)
{
  RCC_OscInitTypeDef RCC_OscInitStruct = {0};
  RCC_ClkInitTypeDef RCC_ClkInitStruct = {0};

  /** Initializes the RCC Oscillators according to the specified parameters
  * in the RCC_OscInitTypeDef structure.
  */
  RCC_OscInitStruct.OscillatorType = RCC_OSCILLATORTYPE_HSI;
  RCC_OscInitStruct.HSIState = RCC_HSI_ON;
  RCC_OscInitStruct.HSICalibrationValue = RCC_HSICALIBRATION_DEFAULT;
  RCC_OscInitStruct.PLL.PLLState = RCC_PLL_NONE;
  if (HAL_RCC_OscConfig(&RCC_OscInitStruct) != HAL_OK)
  {
    Error_Handler();
  }

  /** Initializes the CPU, AHB and APB buses clocks
  */
  RCC_ClkInitStruct.ClockType = RCC_CLOCKTYPE_HCLK|RCC_CLOCKTYPE_SYSCLK
                              |RCC_CLOCKTYPE_PCLK1|RCC_CLOCKTYPE_PCLK2;
  RCC_ClkInitStruct.SYSCLKSource = RCC_SYSCLKSOURCE_HSI;
  RCC_ClkInitStruct.AHBCLKDivider = RCC_SYSCLK_DIV1;
  RCC_ClkInitStruct.APB1CLKDivider = RCC_HCLK_DIV2;
  RCC_ClkInitStruct.APB2CLKDivider = RCC_HCLK_DIV1;

  if (HAL_RCC_ClockConfig(&RCC_ClkInitStruct, FLASH_LATENCY_0) != HAL_OK)
  {
    Error_Handler();
  }
}

/**
  * @brief CAN Initialization Function
  * @param None
  * @retval None
  */
static void MX_CAN_Init(void)
{

  /* USER CODE BEGIN CAN_Init 0 */

  /* USER CODE END CAN_Init 0 */

  /* USER CODE BEGIN CAN_Init 1 */

  /* USER CODE END CAN_Init 1 */
  hcan.Instance = CAN;
  hcan.Init.Prescaler = 1;
  hcan.Init.Mode = CAN_MODE_NORMAL;
  hcan.Init.SyncJumpWidth = CAN_SJW_1TQ;
  hcan.Init.TimeSeg1 = CAN_BS1_2TQ;
  hcan.Init.TimeSeg2 = CAN_BS2_1TQ;
  hcan.Init.TimeTriggeredMode = DISABLE;
  hcan.Init.AutoBusOff = DISABLE;
  hcan.Init.AutoWakeUp = DISABLE;
  hcan.Init.AutoRetransmission = DISABLE;
  hcan.Init.ReceiveFifoLocked = DISABLE;
  hcan.Init.TransmitFifoPriority = DISABLE;
  if (HAL_CAN_Init(&hcan) != HAL_OK)
  {
    Error_Handler();
  }
  /* USER CODE BEGIN CAN_Init 2 */

  /* USER CODE END CAN_Init 2 */

}

/**
  * @brief USART2 Initialization Function
  * @param None
  * @retval None
  */
static void MX_USART2_UART_Init(void)
{

  /* USER CODE BEGIN USART2_Init 0 */

  /* USER CODE END USART2_Init 0 */

  /* USER CODE BEGIN USART2_Init 1 */

  /* USER CODE END USART2_Init 1 */
  huart2.Instance = USART2;
  huart2.Init.BaudRate = 115200;
  huart2.Init.WordLength = UART_WORDLENGTH_8B;
  huart2.Init.StopBits = UART_STOPBITS_1;
  huart2.Init.Parity = UART_PARITY_NONE;
  huart2.Init.Mode = UART_MODE_TX_RX;
  huart2.Init.HwFlowCtl = UART_HWCONTROL_NONE;
  huart2.Init.OverSampling = UART_OVERSAMPLING_16;
  huart2.Init.OneBitSampling = UART_ONE_BIT_SAMPLE_DISABLE;
  huart2.AdvancedInit.AdvFeatureInit = UART_ADVFEATURE_NO_INIT;
  if (HAL_UART_Init(&huart2) != HAL_OK)
  {
    Error_Handler();
  }
  /* USER CODE BEGIN USART2_Init 2 */

  /* USER CODE END USART2_Init 2 */

}

/**
  * @brief GPIO Initialization Function
  * @param None
  * @retval None
  */
static void MX_GPIO_Init(void)
{
  /* USER CODE BEGIN MX_GPIO_Init_1 */

  /* USER CODE END MX_GPIO_Init_1 */

  /* GPIO Ports Clock Enable */
  __HAL_RCC_GPIOF_CLK_ENABLE();
  __HAL_RCC_GPIOA_CLK_ENABLE();

  /* USER CODE BEGIN MX_GPIO_Init_2 */

  /* USER CODE END MX_GPIO_Init_2 */
}

/* USER CODE BEGIN 4 */

/* USER CODE END 4 */

/**
  * @brief  This function is executed in case of error occurrence.
  * @retval None
  */
void Error_Handler(void)
{
  /* USER CODE BEGIN Error_Handler_Debug */
  /* User can add his own implementation to report the HAL error return state */
  __disable_irq();
  while (1)
  {
  }
  /* USER CODE END Error_Handler_Debug */
}
#ifdef USE_FULL_ASSERT
/**
  * @brief  Reports the name of the source file and the source line number
  *         where the assert_param error has occurred.
  * @param  file: pointer to the source file name
  * @param  line: assert_param error line source number
  * @retval None
  */
void assert_failed(uint8_t *file, uint32_t line)
{
  /* USER CODE BEGIN 6 */
  /* User can add his own implementation to report the file name and line number,
     ex: printf("Wrong parameters value: file %s on line %d\r\n", file, line) */
  /* USER CODE END 6 */
}
#endif /* USE_FULL_ASSERT */
