# 游戏体验包切换方案

手游的开发过程中，经常会出一些开发包（有 GM 功能，有选服功能等）或其他形式的体验包。
传统方式是通过修改代码中某个常量或宏来实现的，这种方式每种体验环境都需要重新构建，浪费人力。就算是自动化构建，也会拖慢进程（尤其在项目很大，每次构建十分钟以上更为明显）。
所以，单一包体可切换体验环境，能有效提高 QA 和研发效率。

## 核心原理

通过 RSA 非对称加密，保证只有开发者能制作代理环境或配置文件，以此来保证高权限环境不会被轻松伪造。

## 思路一（通过网络环境切换）

优点

- 单包多环境，切换快
- 网络环境配置只需要搞一次

缺点

- 配置网络环境有一定门槛（尤其对于非研发组人员，如渠道，市场等）
- 外网体验需要部署外网代理

## 思路二（通过配置文件切换

优点

- 无上手门槛，不需要部署代理环境
- 环境判断不受网络延迟影响

缺点

- 每个环境单独的包（借助自动化构建和包体的增量存储可大幅研发工作流）