/**
 * @param { import("knex").Knex } knex
 * @returns { Promise<void> }
 */
exports.seed = async function(knex) {
  // Xóa dữ liệu cũ trước khi thêm mới
  await knex('MaterialItem').del();
  await knex('BillInfo').del();
  await knex('Bill').del();
  await knex('ProductMaterial').del();
  await knex('Product').del();
  await knex('Material').del();
  await knex('Category').del();
  await knex('Account').del();
  await knex('CafeTable').del();

  // Thêm dữ liệu cho bảng CafeTable
  // await knex('CafeTable').insert([
  //   { name: 'Bàn 1', status: 'Trống', location: 'Tầng 1' },
  //   { name: 'Bàn 2', status: 'Có khách', location: 'Tầng 1' },
  //   { name: 'Bàn 3', status: 'Trống', location: 'Tầng 2' },
  // ]);

  const cafeTables = Array.from({ length: 20 }, (_, i) => ({
    name: `Bàn ${i + 1}`,
    status: i % 3 === 0 ? 'Trống' : 'Có khách',
    location: i < 10 ? 'Tầng 1' : 'Tầng 2',
  }));

  await knex('CafeTable').insert(cafeTables);

  // Thêm dữ liệu cho bảng Account
  await knex('Account').insert([
    { userName: 'admin', displayName: 'Admin', password: 'admin123', type: 1 },
    { userName: 'staff1', displayName: 'Nhân viên 1', password: 'staff123', type: 0 },
  ]);

  // Thêm dữ liệu cho bảng Category
  await knex('Category').insert([
    { name: 'Cà phê', description: 'Các loại cà phê' },
    { name: 'Trà', description: 'Các loại trà' },
  ]);

  // Thêm dữ liệu cho bảng Material
  await knex('Material').insert([
    { 
      name: 'Cà phê hạt', 
      unit: 'kg', 
      currentStock: 10, 
      minStock: 2, 
      price: 200000,
      imageUrl: '/Assets/Material/ca-phe-hat.jpeg' 
    },
    { 
      name: 'Trà xanh', 
      unit: 'gói', 
      currentStock: 20, 
      minStock: 5, 
      price: 50000,
      imageUrl: '/Assets/Material/tra-xanh.jpg' 
    },
  ]);

  // Thêm dữ liệu cho bảng Product
  await knex('Product').insert([
    { name: 'Cà phê sữa', idCategory: 1, price: 30000, description: 'Cà phê pha sữa đặc', isAvailable: true, imageUrl: '/Assets/Product/cafe-sua.jpeg'  },
    { name: 'Trà chanh', idCategory: 2, price: 25000, description: 'Trà xanh pha chanh', isAvailable: true, imageUrl: '/Assets/Product/tra-chanh.jpg' },
  ]);

  // Thêm dữ liệu cho bảng ProductMaterial
  await knex('ProductMaterial').insert([
    { idProduct: 1, idMaterial: 1, quantity: 0.05 }, // 50g cà phê hạt cho 1 ly
    { idProduct: 2, idMaterial: 2, quantity: 1 }, // 1 gói trà xanh cho 1 ly
  ]);

  // Thêm dữ liệu cho bảng Bill
  await knex('Bill').insert([
    { dateCheckIn: knex.fn.now(), idTable: 2, status: 1, totalAmount: 80000, paymentMethod: 'Tiền mặt', finalAmount: 80000 },
    { dateCheckIn: knex.fn.now(), idTable: 1, status: 0, totalAmount: 755000, paymentMethod: 'Tiền mặt', finalAmount: 755000 },
    { dateCheckIn: knex.fn.now(), idTable: 3, status: 0, totalAmount: 50000, paymentMethod: 'Tiền mặt', finalAmount: 50000 },
  ]);

  // Thêm dữ liệu cho bảng BillInfo
  await knex('BillInfo').insert([
    { idBill: 2, idProduct: 1, count: 1, unitPrice: 30000, totalPrice: 30000 },
    { idBill: 2, idProduct: 2, count: 2, unitPrice: 25000, totalPrice: 25000 },
    { idBill: 3, idProduct: 2, count: 2, unitPrice: 50000, totalPrice: 50000 },
  ]);

  // Thêm dữ liệu cho bảng MaterialItem
  await knex('MaterialItem').insert([
    { idMaterial: 1, type: 'Import', quantity: 5, unitPrice: 200000, note: 'Nhập hàng tháng 3' },
    { idMaterial: 2, type: 'Import', quantity: 10, unitPrice: 50000, note: 'Nhập hàng tháng 3' },
  ]);
};