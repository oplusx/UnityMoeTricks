# UI Extension UGUI的扩展

## UIPolygonImage 不规则多边形图片

* 利用PolygonCollider2D实现不规则形状的Raycast响应
* 利用SpriteEditor生成Custom Physics Shape数据，再将并数据填充到PolygonCollider2D
* 预处理数据效果更佳，提供physicsFactorY和physicsFactorX矫正缩放问题，并搭配GeneratePolygonFromSprite按钮一起使用。