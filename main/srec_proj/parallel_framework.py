from multiprocessing import Pool
from cpuinfo import get_cpu_info
from typing import List
import sys


def parallel_process_framework(
    func,
    flags,
    log_parallel: bool = False,
    log_debug: bool = False,
    processor_ratio: float = 0.45,
    log_retry: bool = False,
    **kwargs,
) -> List:
    """
    平行運算流程框架
    func 為要啟動的函式
    flags = [
        [arg1, kwargs1],
        [arg2, kwargs2],
        [arg3, kwargs3],
        [arg4, kwargs4],
        [arg5, kwargs5],
        [arg6, kwargs6],
    ]
    log_parallel 為確認是否啟動平行
    process_ratio 為平行運算資源的比例

    log_retry
        如果前一次為平行運算, 如果 log_retry = True
        當失敗無法計算時, 則再啟動一次循序計算
    """
    root_logger = kwargs.get("root_logger", None)
    result = []

    if root_logger is not None:
        root_logger.debug(
            "log parallel: {}".format(log_parallel)
        )

    if log_parallel:
        cpu_item = get_cpu_info()
        # 計算可用的 CPU 數量
        process_size = max(
            int(float(cpu_item["count"]) * processor_ratio),
            1,
        )
        if root_logger is not None:
            root_logger.debug(
                "process_size: {}".format(process_size)
            )
        try:
            with Pool(processes=process_size) as pool:
                result = pool.map(func, flags)
                pool.close()  # Close the pool to not create new process
                pool.join()  # Make main process to wait for the pool
        except AssertionError:
            # AssertionError: daemonic processes are not allowed to have children
            if log_retry:
                if root_logger is not None:
                    root_logger.debug(
                        "平行處理計算失敗, 改用單一處理"
                    )
                return parallel_process_framework(
                    func,
                    flags,
                    log_parallel=False,
                    log_debug=False,
                    processor_ratio=processor_ratio,
                    **kwargs,
                )
    else:
        # print("  -- 循序分段寫入")
        for flag in flags:
            result.append(func(flag))
            if log_debug:
                sys.exit()
    return result
