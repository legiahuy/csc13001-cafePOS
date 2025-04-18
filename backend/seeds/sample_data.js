/**
 * @param { import("knex").Knex } knex
 * @returns { Promise<void> }
 */
exports.seed = async function(knex) {
  // Xóa dữ liệu cũ trước khi thêm mới
  await knex('MaterialItem').del();
  await knex('BillInfo').del();
  await knex('Bill').del();
  await knex('PaymentMethod').del();
  await knex('Guest').del();
  await knex('ProductMaterial').del();
  await knex('Product').del();
  await knex('Material').del();
  await knex('Category').del();
  await knex('Staff').del();
  await knex('Account').del();
  await knex('CafeTable').del();

  // Thêm dữ liệu cho bảng CafeTable
  const cafeTables = Array.from({ length: 20 }, (_, i) => ({
    name: `Bàn ${i + 1}`,
    status: i % 3 === 0 ? 'Trống' : 'Có khách',
    location: i < 10 ? 'Tầng 1' : 'Tầng 2',
  }));

  await knex('CafeTable').insert(cafeTables);

  // Thêm dữ liệu cho bảng Account
  await knex('Account').insert([
    { userName: 'admin', displayName: 'Admin', password: 'admin123', type: 1 },
    {
      userName: 'staff1',
      displayName: 'Phạm Hồng Minh',
      password: '123', 
      type: 0 
    },
    {
      userName: 'staff2',
      displayName: 'Nguyễn Thị Lan',
      password: '123',
      type: 0 
    },
    {
      userName: 'staff3',
      displayName: 'Trần Quốc Bảo',
      password: '123',
      type: 0
    }
  ]);

  // Thêm dữ liệu cho bảng Staff
  await knex('Staff').insert([
    {
      name: 'Phạm Hồng Minh',
      dob: '1990-04-12',
      gender: 'Male',
      phone: '0909123456',
      email: 'minh.pham@example.com',
      position: 'Thu ngân',
      userName: 'staff1',
      salary: 15000000
    },
    {
      name: 'Nguyễn Thị Lan',
      dob: '1995-09-30',
      gender: 'Female',
      phone: '0934567890',
      email: 'lan.nguyen@example.com',
      position: 'Thu ngân',
      userName: 'staff2',
      salary: 8500000
    },
    {
      name: 'Trần Quốc Bảo',
      dob: '1998-06-22',
      gender: 'Male',
      phone: '0987123456',
      email: 'bao.tran@example.com',
      position: 'Phục vụ',
      userName: 'staff3',
      salary: 7000000
    }
  ]);

  // Thêm dữ liệu cho bảng Guest
  await knex('Guest').insert([
    {
      name: 'Lê Thị Hương',
      phone: '0912345678',
      email: 'huong.le@example.com',
      totalPoints: 150,
      availablePoints: 150,
      memberSince: '2024-01-15',
      membershipLevel: 'Bạc',
      notes: 'Khách hàng thường xuyên'
    },
    {
      name: 'Đỗ Văn Nam',
      phone: '0987654321',
      email: 'nam.do@example.com',
      totalPoints: 320,
      availablePoints: 320,
      memberSince: '2023-11-20',
      membershipLevel: 'Vàng',
      notes: 'Thích đồ uống không đường'
    },
    {
      name: 'Trần Thanh Hà',
      phone: '0976543210',
      email: 'ha.tran@example.com',
      totalPoints: 50,
      availablePoints: 50,
      memberSince: '2024-03-05',
      membershipLevel: 'Regular',
      notes: ''
    }
  ]);
  

  // Thêm dữ liệu cho bảng PaymentMethod
  await knex('PaymentMethod').insert([
    {
      name: 'Tiền mặt',
      description: 'Thanh toán bằng tiền mặt',
      isActive: true,
      iconUrl: '/Assets/Payment/cash.jpg',
      processingFee: 0,
      requiresVerification: false
    },
    {
      name: 'Thẻ ngân hàng',
      description: 'Thanh toán bằng thẻ ATM/Visa/Mastercard',
      isActive: true,
      iconUrl: '/Assets/Payment/card.png',
      processingFee: 0.015, // 1.5% fee
      requiresVerification: true
    },
    {
      name: 'Momo',
      description: 'Thanh toán qua ví điện tử Momo',
      isActive: false,
      iconUrl: '/Assets/Payment/momo.png',
      processingFee: 0.01, // 1% fee
      requiresVerification: false
    },
    {
      name: 'ZaloPay',
      description: 'Thanh toán qua ví điện tử ZaloPay',
      isActive: true,
      iconUrl: '/Assets/Payment/zalopay.png',
      processingFee: 0.01, // 1% fee
      requiresVerification: false
    }
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
    { 
      dateCheckIn: knex.fn.now(), 
      idTable: 2, 
      idStaff: 1, 
      idGuest: 1, 
      paymentMethod: 1, // Tiền mặt
      status: 1, 
      totalAmount: 80000, 
      finalAmount: 80000 
    },
    { 
      dateCheckIn: knex.fn.now(), 
      idTable: 1, 
      idStaff: 2, 
      idGuest: 2, 
      paymentMethod: 3, // Momo
      status: 0, 
      totalAmount: 755000, 
      finalAmount: 755000,
      paymentNotes: 'Thanh toán qua Momo SĐT: 0987654321'
    },
    { 
      dateCheckIn: knex.fn.now(), 
      idTable: 3, 
      idStaff: 3, 
      paymentMethod: 2, // Thẻ ngân hàng
      status: 0, 
      totalAmount: 50000, 
      finalAmount: 50000,
      paymentNotes: 'Thẻ Vietcombank'
    },
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