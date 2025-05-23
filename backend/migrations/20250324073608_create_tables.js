/**
 * @param { import("knex").Knex } knex
 * @returns { Promise<void> }
 */
exports.up = function(knex) {
  return knex.schema
    .createTable('CafeTable', (table) => {
      table.increments('id').primary();
      table.string('name', 100).notNullable().defaultTo('Bàn mới');
      table.string('status', 100).notNullable().defaultTo('Trống');
      table.string('location', 200);
    })
    .createTable('Account', (table) => {
      table.string('userName', 100).primary();
      table.string('displayName', 100).notNullable();
      table.string('password', 1000).notNullable().defaultTo('0');
      table.integer('type').defaultTo(0);
    })
    .createTable('Category', (table) => {
      table.increments('id').primary();
      table.string('name', 100).defaultTo('Chưa phân loại');
      table.string('description', 500);
    })
    .createTable('Material', (table) => {
      table.increments('id').primary();
      table.string('name', 100).notNullable();
      table.string('unit', 50).notNullable();
      table.integer('currentStock').notNullable().defaultTo(0);
      table.integer('minStock').notNullable().defaultTo(0);
      table.float('price').notNullable().defaultTo(0);
      table.string('description', 500);
      table.string('imageUrl', 255);
    })
    .createTable('Product', (table) => {
      table.increments('id').primary();
      table.string('name', 100).notNullable().defaultTo('Món mới');
      table.integer('idCategory').notNullable().references('id').inTable('Category').onDelete('CASCADE');
      table.float('price').notNullable().defaultTo(0);
      table.string('description', 500);
      table.boolean('isAvailable').notNullable().defaultTo(true);
      table.string('imageUrl', 255);
    })
    .createTable('ProductMaterial', (table) => {
      table.increments('id').primary();
      table.integer('idProduct').notNullable().references('id').inTable('Product').onDelete('CASCADE');
      table.integer('idMaterial').notNullable().references('id').inTable('Material').onDelete('CASCADE');
      table.float('quantity').notNullable();
    })
    .createTable('Staff', (table) => {
      table.increments('id').primary();
      table.string('name', 100).notNullable();
      table.date('dob');
      table.string('gender', 10);
      table.string('phone', 20);
      table.string('email', 100);
      table.string('position', 50);
      table.string('userName', 100)
          .references('userName') 
          .inTable('Account')
          .onDelete('SET NULL');  
      table.float('salary').defaultTo(0);
    })
    .createTable('Guest', (table) => {
      table.increments('id').primary();
      table.string('name', 100).notNullable();
      table.string('phone', 20);
      table.string('email', 100);
      table.integer('totalPoints').notNullable().defaultTo(0);
      table.integer('availablePoints').notNullable().defaultTo(0);
      table.date('memberSince').notNullable().defaultTo(knex.fn.now());
      table.string('membershipLevel', 50).defaultTo('Regular');
      table.string('notes', 500);
    })
    .createTable('PaymentMethod', (table) => {
      table.increments('id').primary();
      table.string('name', 100).notNullable();
      table.string('description', 500);
      table.boolean('isActive').notNullable().defaultTo(true);
      table.string('iconUrl', 255);
      table.float('processingFee').defaultTo(0); // For tracking fees if applicable
      table.boolean('requiresVerification').defaultTo(false);
    })
    .createTable('Bill', (table) => {
      table.increments('id').primary();
      table.datetime('dateCheckIn').notNullable().defaultTo(knex.fn.now());
      table.datetime('dateCheckOut');
      table.integer('idTable').notNullable().references('id').inTable('CafeTable').onDelete('CASCADE');
      table.integer('idStaff').notNullable().references('id').inTable('Staff').onDelete('RESTRICT');
      table.integer('idGuest').references('id').inTable('Guest').onDelete('SET NULL');
      table.integer('paymentMethod').references('id').inTable('PaymentMethod').onDelete('RESTRICT');
      table.integer('status').notNullable().defaultTo(0);
      table.float('totalAmount').notNullable().defaultTo(0);
      table.float('discount').defaultTo(0);
      table.float('finalAmount').notNullable().defaultTo(0);
      table.string('paymentNotes', 500);
    })
    .createTable('BillInfo', (table) => {
      table.increments('id').primary();
      table.integer('idBill').notNullable().references('id').inTable('Bill').onDelete('CASCADE');
      table.integer('idProduct').notNullable().references('id').inTable('Product').onDelete('CASCADE');
      table.integer('count').notNullable().defaultTo(0);
      table.float('unitPrice').notNullable();
      table.float('totalPrice').notNullable();
    })
    .createTable('MaterialItem', (table) => {
      table.increments('id').primary();
      table.integer('idMaterial').notNullable().references('id').inTable('Material').onDelete('CASCADE');
      table.string('type', 50).notNullable(); // 'Import', 'Export', 'Consume'
      table.float('quantity').notNullable();
      table.float('unitPrice').notNullable();
      table.datetime('date').notNullable().defaultTo(knex.fn.now());
      table.string('note', 500);
    })
    .createTable('customer_feedback', function(table) {
      table.increments('id').primary();
      table.string('name', 100).notNullable();
      table.string('email', 100).notNullable();
      table.text('content').notNullable();
      table.timestamp('submitted_at').defaultTo(knex.fn.now());
      table.enum('status', ['pending', 'in_progress', 'completed']).defaultTo('pending');
      table.text('response_content').nullable();
      table.timestamp('responded_at').nullable();
      table.integer('staff_id').nullable().references('id').inTable('Staff');
      table.timestamp('created_at').defaultTo(knex.fn.now());
      table.timestamp('updated_at').defaultTo(knex.fn.now());
    });
};

/**
* @param { import("knex").Knex } knex
* @returns { Promise<void> }
*/
exports.down = function(knex) {
  return knex.schema
    .dropTableIfExists('MaterialItem')
    .dropTableIfExists('BillInfo')
    .dropTableIfExists('Bill')
    .dropTableIfExists('PaymentMethod')
    .dropTableIfExists('Guest')
    .dropTableIfExists('ProductMaterial')
    .dropTableIfExists('Product')
    .dropTableIfExists('Material')
    .dropTableIfExists('Category')
    .dropTableIfExists('CafeTable')
    .dropTableIfExists('customer_feedback')
    .dropTableIfExists('Staff')
    .dropTableIfExists('Account');
};