SELECT 
    dbo.Users.UserId, 
    dbo.Users.UserName, 
    dbo.Users.Email, 
    dbo.Users.IsDeleted AS UserIsDeleted,
    dbo.Products.ProductId, 
    dbo.Products.ProductName, 
    dbo.Products.IsDeleted AS ProductIsDeleted, 
    dbo.CartProducts.CartProductId, 
    dbo.CartProducts.ProductId, 
    dbo.CartProducts.CartId, 
    dbo.CartProducts.Quantity, 
    dbo.Carts.CartId, 
    dbo.Carts.UserId, 
    dbo.Carts.IsDeleted AS CartIsDeleted
FROM 
    dbo.CartProducts 
    INNER JOIN dbo.Products ON dbo.CartProducts.ProductId = dbo.Products.ProductId 
    INNER JOIN dbo.Carts ON dbo.CartProducts.CartId = dbo.Carts.CartId 
    INNER JOIN dbo.Users ON dbo.Carts.UserId = dbo.Users.UserId;
