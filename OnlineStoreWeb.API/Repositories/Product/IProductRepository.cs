interface IProductRepository
{
    List<Product> GetAllProducts();
    Product GetProductWithId(int id);
    void AddProduct(CreateProductDto createProductDto);
    void UpdateProduct(UpdateProductDto updateProductDto);
    void DeleteProduct(int id);
}