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
  await knex('Account').del();
  await knex('CafeTable').del();
  await knex('customer_feedback').del();
  await knex('Staff').del();

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
    { name: 'Nước ép', description: 'Nước ép trái cây tươi' },
    { name: 'Đồ ăn nhẹ', description: 'Các món ăn nhẹ' },
    { name: 'Bánh ngọt', description: 'Các loại bánh ngọt' }
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
    { 
      name: 'Sữa tươi', 
      unit: 'lít', 
      currentStock: 15, 
      minStock: 3, 
      price: 35000,
      imageUrl: '/Assets/Material/sua-tuoi.jpg' 
    },
    { 
      name: 'Cam', 
      unit: 'kg', 
      currentStock: 8, 
      minStock: 2, 
      price: 40000,
      imageUrl: '/Assets/Material/cam.jpg' 
    },
    { 
      name: 'Dâu tây', 
      unit: 'kg', 
      currentStock: 5, 
      minStock: 1, 
      price: 120000,
      imageUrl: '/Assets/Material/dau-tay.jpg' 
    }
  ]);

  // Thêm dữ liệu cho bảng Product
  await knex('Product').insert([
    {
      name: 'Cà phê đen',
      idCategory: 1, // Cà phê
      price: 25000,
      description: 'Cà phê đen truyền thống',
      isAvailable: true,
      imageUrl: '/Assets/Product/ca-phe-den.jpg'
    },
    {
      name: 'Cà phê sữa',
      idCategory: 1, // Cà phê
      price: 30000,
      description: 'Cà phê sữa đậm đà',
      isAvailable: true,
      imageUrl: '/Assets/Product/ca-phe-sua.jpg'
    },
    {
      name: 'Trà chanh',
      idCategory: 2, // Trà
      price: 20000,
      description: 'Trà chanh mát lạnh',
      isAvailable: true,
      imageUrl: '/Assets/Product/tra-chanh.jpg'
    },
    {
      name: 'Trà đào',
      idCategory: 2, // Trà
      price: 35000,
      description: 'Trà đào thơm ngon',
      isAvailable: true,
      imageUrl: '/Assets/Product/tra-dao.jpg'
    },
    {
      name: 'Nước cam',
      idCategory: 3, // Nước ép
      price: 35000,
      description: 'Nước cam tươi',
      isAvailable: true,
      imageUrl: '/Assets/Product/nuoc-cam.jpg'
    },
    {
      name: 'Bánh tiramisu',
      idCategory: 5, // Bánh ngọt
      price: 45000,
      description: 'Bánh tiramisu Ý',
      isAvailable: true,
      imageUrl: '/Assets/Product/tiramisu.jpg'
    },
    {
      name: 'Bánh croissant',
      idCategory: 5, // Bánh ngọt
      price: 35000,
      description: 'Bánh croissant Pháp',
      isAvailable: true,
      imageUrl: '/Assets/Product/croissant.jpg'
    },
    {
      name: 'Sandwich',
      idCategory: 4, // Đồ ăn nhẹ
      price: 40000,
      description: 'Sandwich gà',
      isAvailable: true,
      imageUrl: '/Assets/Product/sandwich.jpg'
    }
  ]);

  // Thêm dữ liệu cho bảng ProductMaterial
  await knex('ProductMaterial').insert([
    { idProduct: 1, idMaterial: 1, quantity: 0.03 }, // 30g cà phê hạt cho 1 ly cà phê đen
    { idProduct: 2, idMaterial: 1, quantity: 0.05 }, // 50g cà phê hạt cho 1 ly cà phê sữa
    { idProduct: 2, idMaterial: 3, quantity: 0.1 }, // 100ml sữa tươi cho 1 ly cà phê sữa
    { idProduct: 3, idMaterial: 2, quantity: 1 }, // 1 gói trà xanh cho 1 ly trà chanh
    { idProduct: 4, idMaterial: 2, quantity: 1 }, // 1 gói trà xanh cho 1 ly trà đào
    { idProduct: 5, idMaterial: 4, quantity: 0.5 }, // 500g cam cho 1 ly nước cam
  ]);

  // Thêm dữ liệu cho bảng Bill với nhiều ngày khác nhau
  const billData = [
    // Hóa đơn các ngày trong tháng 1/2024
    { 
      dateCheckIn: '2024-01-05 08:30:00', 
      dateCheckOut: '2024-01-05 09:15:00',
      idTable: 2, 
      idStaff: 1, 
      idGuest: 1, 
      paymentMethod: 1, // Tiền mặt
      status: 1, // Đã thanh toán
      totalAmount: 55000, 
      finalAmount: 55000 
    },
    { 
      dateCheckIn: '2024-01-08 14:20:00', 
      dateCheckOut: '2024-01-08 15:45:00',
      idTable: 5, 
      idStaff: 2, 
      paymentMethod: 3, // Momo
      status: 1, // Đã thanh toán
      totalAmount: 95000, 
      finalAmount: 95000,
      paymentNotes: 'Thanh toán qua Momo'
    },
    { 
      dateCheckIn: '2024-01-15 18:45:00', 
      dateCheckOut: '2024-01-15 20:10:00',
      idTable: 10, 
      idStaff: 3, 
      idGuest: 2,
      paymentMethod: 2, // Thẻ ngân hàng
      status: 1, // Đã thanh toán
      totalAmount: 180000, 
      finalAmount: 180000,
      paymentNotes: 'Thẻ Vietinbank'
    },
    { 
      dateCheckIn: '2024-01-25 10:15:00', 
      dateCheckOut: '2024-01-25 11:30:00',
      idTable: 7, 
      idStaff: 1, 
      paymentMethod: 4, // ZaloPay
      status: 1, // Đã thanh toán
      totalAmount: 75000, 
      finalAmount: 75000,
      paymentNotes: 'ZaloPay'
    },
    
    // Hóa đơn các ngày trong tháng 2/2024
    { 
      dateCheckIn: '2024-02-02 09:00:00', 
      dateCheckOut: '2024-02-02 10:20:00',
      idTable: 3, 
      idStaff: 2, 
      idGuest: 3,
      paymentMethod: 1, // Tiền mặt
      status: 1, // Đã thanh toán
      totalAmount: 110000, 
      finalAmount: 110000 
    },
    { 
      dateCheckIn: '2024-02-12 16:30:00', 
      dateCheckOut: '2024-02-12 17:45:00',
      idTable: 9, 
      idStaff: 3, 
      paymentMethod: 3, // Momo
      status: 1, // Đã thanh toán
      totalAmount: 65000, 
      finalAmount: 65000,
      paymentNotes: 'Thanh toán qua Momo'
    },
    { 
      dateCheckIn: '2024-02-20 12:45:00', 
      dateCheckOut: '2024-02-20 14:15:00',
      idTable: 12, 
      idStaff: 1, 
      idGuest: 1,
      paymentMethod: 2, // Thẻ ngân hàng
      status: 1, // Đã thanh toán
      totalAmount: 155000, 
      finalAmount: 155000,
      paymentNotes: 'Thẻ BIDV'
    },
    
    // Hóa đơn các ngày trong tháng 3/2024
    { 
      dateCheckIn: '2024-03-05 11:30:00', 
      dateCheckOut: '2024-03-05 12:45:00',
      idTable: 6, 
      idStaff: 2, 
      paymentMethod: 1, // Tiền mặt
      status: 1, // Đã thanh toán
      totalAmount: 90000, 
      finalAmount: 90000 
    },
    { 
      dateCheckIn: '2024-03-15 15:15:00', 
      dateCheckOut: '2024-03-15 16:30:00',
      idTable: 4, 
      idStaff: 3, 
      idGuest: 2,
      paymentMethod: 4, // ZaloPay
      status: 1, // Đã thanh toán
      totalAmount: 140000, 
      finalAmount: 140000,
      paymentNotes: 'ZaloPay'
    },
    { 
      dateCheckIn: '2024-03-24 18:00:00', 
      dateCheckOut: '2024-03-24 19:45:00',
      idTable: 15, 
      idStaff: 1, 
      paymentMethod: 1, // Tiền mặt
      status: 1, // Đã thanh toán
      totalAmount: 210000, 
      finalAmount: 210000 
    },
    
    // Hóa đơn các ngày trong tháng 4/2024
    { 
      dateCheckIn: '2024-04-03 10:45:00', 
      dateCheckOut: '2024-04-03 12:00:00',
      idTable: 8, 
      idStaff: 2, 
      idGuest: 3,
      paymentMethod: 3, // Momo
      status: 1, // Đã thanh toán
      totalAmount: 120000, 
      finalAmount: 120000,
      paymentNotes: 'Thanh toán qua Momo'
    },
    { 
      dateCheckIn: '2024-04-12 14:00:00', 
      dateCheckOut: '2024-04-12 15:30:00',
      idTable: 11, 
      idStaff: 3, 
      paymentMethod: 2, // Thẻ ngân hàng
      status: 1, // Đã thanh toán
      totalAmount: 85000, 
      finalAmount: 85000,
      paymentNotes: 'Thẻ Techcombank'
    },
    { 
      dateCheckIn: '2024-04-19 17:30:00', 
      idTable: 1, 
      idStaff: 2, 
      idGuest: 2, 
      paymentMethod: 3, // Momo
      status: 0, // Chưa thanh toán
      totalAmount: 150000, 
      finalAmount: 150000
    },
    { 
      dateCheckIn: '2024-04-19 16:45:00', 
      idTable: 3, 
      idStaff: 3, 
      paymentMethod: 2, // Thẻ ngân hàng
      status: 0, // Chưa thanh toán
      totalAmount: 95000, 
      finalAmount: 95000
    },

    // Bills for yesterday
    { 
      dateCheckIn: '2025-04-19 09:30:00', 
      dateCheckOut: '2025-04-19 10:15:00',
      idTable: 4, 
      idStaff: 1, 
      paymentMethod: 1, // Tiền mặt
      status: 1, // Đã thanh toán
      totalAmount: 85000, 
      finalAmount: 85000 
    },
    { 
      dateCheckIn: '2025-04-19 14:00:00', 
      dateCheckOut: '2025-04-19 15:30:00',
      idTable: 7, 
      idStaff: 2, 
      paymentMethod: 3, // Momo
      status: 1, // Đã thanh toán
      totalAmount: 155000, 
      finalAmount: 155000,
      paymentNotes: 'Thanh toán qua Momo'
    },
    { 
      dateCheckIn: '2025-04-19 18:45:00', 
      dateCheckOut: '2025-04-19 20:00:00',
      idTable: 12, 
      idStaff: 3, 
      paymentMethod: 2, // Thẻ ngân hàng
      status: 1, // Đã thanh toán
      totalAmount: 210000, 
      finalAmount: 210000,
      paymentNotes: 'Thẻ Vietcombank'
    },

    // Bills for last week
    { 
      dateCheckIn: '2025-04-12 10:00:00', 
      dateCheckOut: '2025-04-12 11:30:00',
      idTable: 5, 
      idStaff: 1, 
      paymentMethod: 1, // Tiền mặt
      status: 1, // Đã thanh toán
      totalAmount: 125000, 
      finalAmount: 125000 
    },
    { 
      dateCheckIn: '2025-04-13 15:30:00', 
      dateCheckOut: '2025-04-13 17:00:00',
      idTable: 8, 
      idStaff: 2, 
      paymentMethod: 4, // ZaloPay
      status: 1, // Đã thanh toán
      totalAmount: 180000, 
      finalAmount: 180000,
      paymentNotes: 'ZaloPay'
    },
    { 
      dateCheckIn: '2025-04-14 19:15:00', 
      dateCheckOut: '2025-04-14 20:45:00',
      idTable: 15, 
      idStaff: 3, 
      paymentMethod: 3, // Momo
      status: 1, // Đã thanh toán
      totalAmount: 245000, 
      finalAmount: 245000,
      paymentNotes: 'Thanh toán qua Momo'
    },

    // Bills for last month (March)
    { 
      dateCheckIn: '2025-03-19 11:30:00', 
      dateCheckOut: '2025-03-19 13:00:00',
      idTable: 6, 
      idStaff: 1, 
      paymentMethod: 1, // Tiền mặt
      status: 1, // Đã thanh toán
      totalAmount: 165000, 
      finalAmount: 165000 
    },
    { 
      dateCheckIn: '2025-03-19 16:45:00', 
      dateCheckOut: '2025-03-19 18:15:00',
      idTable: 9, 
      idStaff: 2, 
      paymentMethod: 2, // Thẻ ngân hàng
      status: 1, // Đã thanh toán
      totalAmount: 195000, 
      finalAmount: 195000,
      paymentNotes: 'Thẻ ACB'
    },
    { 
      dateCheckIn: '2025-03-19 20:00:00', 
      dateCheckOut: '2025-03-19 21:30:00',
      idTable: 14, 
      idStaff: 3, 
      paymentMethod: 3, // Momo
      status: 1, // Đã thanh toán
      totalAmount: 275000, 
      finalAmount: 275000,
      paymentNotes: 'Thanh toán qua Momo'
    }
  ];

  const bills = await knex('Bill').insert(billData).returning('id');

  // Thêm dữ liệu cho bảng BillInfo với các sản phẩm tương ứng
  const billInfoData = [
    // Bill 1 (Jan 05)
    { idBill: bills[0].id, idProduct: 2, count: 1, unitPrice: 30000, totalPrice: 30000 },
    { idBill: bills[0].id, idProduct: 3, count: 1, unitPrice: 25000, totalPrice: 25000 },
    
    // Bill 2 (Jan 08)
    { idBill: bills[1].id, idProduct: 5, count: 1, unitPrice: 40000, totalPrice: 40000 },
    { idBill: bills[1].id, idProduct: 8, count: 1, unitPrice: 30000, totalPrice: 30000 },
    { idBill: bills[1].id, idProduct: 3, count: 1, unitPrice: 25000, totalPrice: 25000 },
    
    // Bill 3 (Jan 15)
    { idBill: bills[2].id, idProduct: 2, count: 2, unitPrice: 30000, totalPrice: 60000 },
    { idBill: bills[2].id, idProduct: 6, count: 2, unitPrice: 45000, totalPrice: 90000 },
    { idBill: bills[2].id, idProduct: 4, count: 1, unitPrice: 35000, totalPrice: 35000 },
    
    // Bill 4 (Jan 25)
    { idBill: bills[3].id, idProduct: 7, count: 1, unitPrice: 35000, totalPrice: 35000 },
    { idBill: bills[3].id, idProduct: 5, count: 1, unitPrice: 40000, totalPrice: 40000 },
    
    // Bill 5 (Feb 02)
    { idBill: bills[4].id, idProduct: 1, count: 2, unitPrice: 25000, totalPrice: 50000 },
    { idBill: bills[4].id, idProduct: 6, count: 1, unitPrice: 45000, totalPrice: 45000 },
    { idBill: bills[4].id, idProduct: 3, count: 1, unitPrice: 25000, totalPrice: 25000 },
    
    // Bill 6 (Feb 12)
    { idBill: bills[5].id, idProduct: 2, count: 1, unitPrice: 30000, totalPrice: 30000 },
    { idBill: bills[5].id, idProduct: 4, count: 1, unitPrice: 35000, totalPrice: 35000 },
    
    // Bill 7 (Feb 20)
    { idBill: bills[6].id, idProduct: 5, count: 2, unitPrice: 40000, totalPrice: 80000 },
    { idBill: bills[6].id, idProduct: 7, count: 1, unitPrice: 35000, totalPrice: 35000 },
    { idBill: bills[6].id, idProduct: 5, count: 1, unitPrice: 40000, totalPrice: 40000 },
    
    // Bill 8 (Mar 05)
    { idBill: bills[7].id, idProduct: 2, count: 2, unitPrice: 30000, totalPrice: 60000 },
    { idBill: bills[7].id, idProduct: 8, count: 1, unitPrice: 30000, totalPrice: 30000 },
    
    // Bill 9 (Mar 15)
    { idBill: bills[8].id, idProduct: 6, count: 2, unitPrice: 45000, totalPrice: 90000 },
    { idBill: bills[8].id, idProduct: 3, count: 2, unitPrice: 25000, totalPrice: 50000 },
    
    // Bill 10 (Mar 24)
    { idBill: bills[9].id, idProduct: 2, count: 3, unitPrice: 30000, totalPrice: 90000 },
    { idBill: bills[9].id, idProduct: 4, count: 2, unitPrice: 35000, totalPrice: 70000 },
    { idBill: bills[9].id, idProduct: 8, count: 1, unitPrice: 30000, totalPrice: 30000 },
    { idBill: bills[9].id, idProduct: 3, count: 1, unitPrice: 25000, totalPrice: 25000 },
    
    // Bill 11 (Apr 03)
    { idBill: bills[10].id, idProduct: 5, count: 2, unitPrice: 40000, totalPrice: 80000 },
    { idBill: bills[10].id, idProduct: 6, count: 1, unitPrice: 45000, totalPrice: 45000 },
    
    // Bill 12 (Apr 12)
    { idBill: bills[11].id, idProduct: 2, count: 1, unitPrice: 30000, totalPrice: 30000 },
    { idBill: bills[11].id, idProduct: 8, count: 1, unitPrice: 30000, totalPrice: 30000 },
    { idBill: bills[11].id, idProduct: 3, count: 1, unitPrice: 25000, totalPrice: 25000 },
    
    // Bill 13 (Apr 19 - Chưa thanh toán)
    { idBill: bills[12].id, idProduct: 2, count: 2, unitPrice: 30000, totalPrice: 60000 },
    { idBill: bills[12].id, idProduct: 6, count: 1, unitPrice: 45000, totalPrice: 45000 },
    { idBill: bills[12].id, idProduct: 5, count: 1, unitPrice: 40000, totalPrice: 40000 },
    
    // Bill 14 (Apr 19 - Chưa thanh toán)
    { idBill: bills[13].id, idProduct: 4, count: 1, unitPrice: 35000, totalPrice: 35000 },
    { idBill: bills[13].id, idProduct: 6, count: 1, unitPrice: 45000, totalPrice: 45000 },
    { idBill: bills[13].id, idProduct: 3, count: 1, unitPrice: 25000, totalPrice: 25000 },

    // Yesterday's bills
    { idBill: bills[14].id, idProduct: 2, count: 2, unitPrice: 30000, totalPrice: 60000 }, // Cà phê sữa
    { idBill: bills[14].id, idProduct: 3, count: 1, unitPrice: 25000, totalPrice: 25000 }, // Trà chanh

    { idBill: bills[15].id, idProduct: 6, count: 2, unitPrice: 45000, totalPrice: 90000 }, // Bánh tiramisu
    { idBill: bills[15].id, idProduct: 5, count: 1, unitPrice: 40000, totalPrice: 40000 }, // Nước cam
    { idBill: bills[15].id, idProduct: 3, count: 1, unitPrice: 25000, totalPrice: 25000 }, // Trà chanh

    { idBill: bills[16].id, idProduct: 2, count: 3, unitPrice: 30000, totalPrice: 90000 }, // Cà phê sữa
    { idBill: bills[16].id, idProduct: 6, count: 2, unitPrice: 45000, totalPrice: 90000 }, // Bánh tiramisu
    { idBill: bills[16].id, idProduct: 4, count: 1, unitPrice: 35000, totalPrice: 35000 }, // Trà đào

    // Last week's bills
    { idBill: bills[17].id, idProduct: 1, count: 2, unitPrice: 25000, totalPrice: 50000 }, // Cà phê đen
    { idBill: bills[17].id, idProduct: 7, count: 1, unitPrice: 35000, totalPrice: 35000 }, // Bánh croissant
    { idBill: bills[17].id, idProduct: 5, count: 1, unitPrice: 40000, totalPrice: 40000 }, // Nước cam

    { idBill: bills[18].id, idProduct: 6, count: 2, unitPrice: 45000, totalPrice: 90000 }, // Bánh tiramisu
    { idBill: bills[18].id, idProduct: 2, count: 2, unitPrice: 30000, totalPrice: 60000 }, // Cà phê sữa
    { idBill: bills[18].id, idProduct: 4, count: 1, unitPrice: 35000, totalPrice: 35000 }, // Trà đào

    { idBill: bills[19].id, idProduct: 6, count: 3, unitPrice: 45000, totalPrice: 135000 }, // Bánh tiramisu
    { idBill: bills[19].id, idProduct: 5, count: 2, unitPrice: 40000, totalPrice: 80000 }, // Nước cam
    { idBill: bills[19].id, idProduct: 4, count: 1, unitPrice: 35000, totalPrice: 35000 }, // Trà đào

    // Last month's bills
    { idBill: bills[20].id, idProduct: 2, count: 3, unitPrice: 30000, totalPrice: 90000 }, // Cà phê sữa
    { idBill: bills[20].id, idProduct: 7, count: 1, unitPrice: 35000, totalPrice: 35000 }, // Bánh croissant
    { idBill: bills[20].id, idProduct: 5, count: 1, unitPrice: 40000, totalPrice: 40000 }, // Nước cam

    { idBill: bills[21].id, idProduct: 6, count: 2, unitPrice: 45000, totalPrice: 90000 }, // Bánh tiramisu
    { idBill: bills[21].id, idProduct: 2, count: 2, unitPrice: 30000, totalPrice: 60000 }, // Cà phê sữa
    { idBill: bills[21].id, idProduct: 8, count: 1, unitPrice: 30000, totalPrice: 30000 }, // Sandwich
    { idBill: bills[21].id, idProduct: 3, count: 1, unitPrice: 25000, totalPrice: 25000 }, // Trà chanh

    { idBill: bills[22].id, idProduct: 6, count: 3, unitPrice: 45000, totalPrice: 135000 }, // Bánh tiramisu
    { idBill: bills[22].id, idProduct: 2, count: 3, unitPrice: 30000, totalPrice: 90000 }, // Cà phê sữa
    { idBill: bills[22].id, idProduct: 5, count: 1, unitPrice: 40000, totalPrice: 40000 }, // Nước cam
    { idBill: bills[22].id, idProduct: 3, count: 1, unitPrice: 25000, totalPrice: 25000 }  // Trà chanh
  ];
  
  await knex('BillInfo').insert(billInfoData);

  // Thêm dữ liệu cho bảng MaterialItem
  await knex('MaterialItem').insert([
    { idMaterial: 1, type: 'Import', quantity: 5, unitPrice: 200000, note: 'Nhập hàng tháng 1'},
    { idMaterial: 2, type: 'Import', quantity: 10, unitPrice: 50000, note: 'Nhập hàng tháng 1'},
    { idMaterial: 3, type: 'Import', quantity: 8, unitPrice: 35000, note: 'Nhập hàng tháng 1'},
    { idMaterial: 1, type: 'Import', quantity: 3, unitPrice: 210000, note: 'Nhập hàng tháng 2'},
    { idMaterial: 4, type: 'Import', quantity: 10, unitPrice: 40000, note: 'Nhập hàng tháng 2'},
    { idMaterial: 5, type: 'Import', quantity: 5, unitPrice: 120000, note: 'Nhập hàng tháng 2'},
    { idMaterial: 2, type: 'Import', quantity: 15, unitPrice: 48000, note: 'Nhập hàng tháng 3'},
    { idMaterial: 3, type: 'Import', quantity: 10, unitPrice: 36000, note: 'Nhập hàng tháng 3'},
    { idMaterial: 1, type: 'Import', quantity: 4, unitPrice: 205000, note: 'Nhập hàng tháng 4'},
    { idMaterial: 4, type: 'Import', quantity: 8, unitPrice: 42000, note: 'Nhập hàng tháng 4'},
    { idMaterial: 2, type: 'Export', quantity: 5, unitPrice: 0, note: 'Xuất hàng hỏng'},
    { idMaterial: 5, type: 'Export', quantity: 2, unitPrice: 0, note: 'Xuất hàng hỏng'}
  ]);

  await knex('customer_feedback').insert([
    {
      name: 'Nguyễn Văn An',
      email: 'nguyenvanan@gmail.com',
      content: 'Tôi rất hài lòng với đồ uống và không gian quán. Thái độ phục vụ của nhân viên rất tốt. Tuy nhiên, wifi hơi chậm và thỉnh thoảng bị ngắt kết nối.',
      submitted_at: knex.raw("NOW() - INTERVAL '2 days'"),
      status: 'completed',
      response_content: 'Kính gửi anh Nguyễn Văn An, Cảm ơn anh đã dành thời gian đánh giá về quán của chúng tôi. Chúng tôi rất vui khi anh hài lòng với đồ uống và không gian quán. Về vấn đề wifi, chúng tôi đã ghi nhận và sẽ nâng cấp hệ thống wifi trong thời gian sớm nhất. Mong được phục vụ anh trong những lần tiếp theo. Trân trọng!',
      responded_at: knex.raw("NOW() - INTERVAL '1 day'"),
      staff_id: 1
    },
    {
      name: 'Trần Thị Bình',
      email: 'tranthibinh@yahoo.com',
      content: 'Cà phê rất ngon, nhưng giá hơi cao so với các quán xung quanh. Tôi đề xuất quán nên có chương trình khuyến mãi, tích điểm cho khách hàng thân thiết.',
      submitted_at: knex.raw("NOW() - INTERVAL '3 days'"),
      status: 'pending',
      response_content: null,
      responded_at: null,
      staff_id: null
    },
    {
      name: 'Lê Minh Cường',
      email: 'leminhcuong@hotmail.com',
      content: 'Tôi đã đặt một bánh sinh nhật tại quán nhưng khi nhận thì bánh không giống với hình ảnh được quảng cáo. Tôi muốn được hoàn tiền hoặc làm lại sản phẩm.',
      submitted_at: knex.raw("NOW() - INTERVAL '12 hours'"),
      status: 'in_progress',
      response_content: null,
      responded_at: null,
      staff_id: 2
    },
    {
      name: 'Phạm Thị Dung',
      email: 'phamthidung@gmail.com',
      content: 'Tôi thích không gian quán rất yên tĩnh, phù hợp để làm việc. Tuy nhiên, quán nên tăng cường ổ cắm điện để khách hàng có thể sạc laptop và điện thoại.',
      submitted_at: knex.raw("NOW() - INTERVAL '5 days'"),
      status: 'completed',
      response_content: 'Chào chị Phạm Thị Dung, Cảm ơn chị đã góp ý cho quán. Chúng tôi rất vui khi không gian quán đáp ứng được nhu cầu làm việc của chị. Về việc bổ sung ổ cắm điện, chúng tôi sẽ lắp đặt thêm ổ cắm tại mỗi bàn trong tuần tới. Mong được đón tiếp chị lần sau. Trân trọng!',
      responded_at: knex.raw("NOW() - INTERVAL '3 days'"),
      staff_id: 3
    },
    {
      name: 'Hoàng Văn Đức',
      email: 'hoangvanduc@gmail.com',
      content: 'Menu quán khá hạn chế, chủ yếu là cà phê. Tôi mong muốn quán có thêm các loại trà sữa và đồ ăn nhẹ.',
      submitted_at: knex.raw("NOW() - INTERVAL '1 day'"),
      status: 'pending',
      response_content: null,
      responded_at: null,
      staff_id: null
    },
    {
      name: 'Vũ Thanh Hà',
      email: 'vuthanhha@outlook.com',
      content: 'Tôi rất thích đồ uống tại quán, đặc biệt là cà phê sữa đá. Tuy nhiên, quán hơi nóng vào buổi trưa, cần bổ sung thêm quạt hoặc điều hòa.',
      submitted_at: knex.raw("NOW() - INTERVAL '4 days'"),
      status: 'completed',
      response_content: 'Kính gửi anh Vũ Thanh Hà, Cảm ơn anh đã đánh giá tích cực về đồ uống của quán. Về vấn đề nhiệt độ quán vào buổi trưa, chúng tôi đã lắp đặt thêm 2 máy điều hòa và sẽ đưa vào sử dụng từ tuần sau. Hi vọng lần tới ghé quán, anh sẽ cảm thấy thoải mái hơn. Trân trọng!',
      responded_at: knex.raw("NOW() - INTERVAL '2 days'"),
      staff_id: 1
    },
    {
      name: 'Ngô Thị Giang',
      email: 'ngothigiang@gmail.com',
      content: 'Nhạc quán quá to, gây khó chịu khi muốn nói chuyện hoặc làm việc. Mong quán điều chỉnh âm lượng phù hợp hơn.',
      submitted_at: knex.raw("NOW()"),
      status: 'pending',
      response_content: null,
      responded_at: null,
      staff_id: null
    },
    {
      name: 'Đỗ Văn Hiệp',
      email: 'dovanhiep@yahoo.com',
      content: 'Tôi là khách hàng thường xuyên của quán. Tôi muốn đề xuất quán mở cửa sớm hơn vào các ngày cuối tuần, khoảng 6h sáng thay vì 7h30.',
      submitted_at: knex.raw("NOW() - INTERVAL '6 hours'"),
      status: 'in_progress',
      response_content: null,
      responded_at: null,
      staff_id: 2
    },
    {
      name: 'Lý Thị Khánh',
      email: 'lythikhanh@gmail.com',
      content: 'Tôi thấy quán có dịch vụ giao hàng nhưng phí ship khá cao. Nếu có thể giảm phí giao hàng hoặc miễn phí cho đơn trên 100k thì tốt quá.',
      submitted_at: knex.raw("NOW() - INTERVAL '2 days'"),
      status: 'completed',
      response_content: 'Chào chị Lý Thị Khánh, Cảm ơn chị đã đóng góp ý kiến về dịch vụ giao hàng của quán. Chúng tôi vừa điều chỉnh chính sách giao hàng: miễn phí giao hàng cho đơn từ 100k trong bán kính 3km và giảm 50% phí giao hàng cho đơn từ 150k trong bán kính 5km. Mong chị tiếp tục ủng hộ quán. Trân trọng!',
      responded_at: knex.raw("NOW() - INTERVAL '1 day'"),
      staff_id: 3
    },
    {
      name: 'Trương Văn Minh',
      email: 'truongvanminh@hotmail.com',
      content: 'Tôi đã thử bánh chocolate của quán nhưng cảm thấy hơi ngọt. Nếu có thể điều chỉnh lượng đường trong các món bánh thì sẽ phù hợp với nhiều khách hàng hơn.',
      submitted_at: knex.raw("NOW() - INTERVAL '8 hours'"),
      status: 'pending',
      response_content: null,
      responded_at: null,
      staff_id: null
    }
  ]);
};