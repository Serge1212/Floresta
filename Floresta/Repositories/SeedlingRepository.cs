using Floresta.Interfaces;
using Floresta.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Floresta.Repositories
{
    public class SeedlingRepository : IRepository<Seedling>
    {
        private FlorestaDbContext _context; 
        public SeedlingRepository(FlorestaDbContext context)
        {
            //отримуємо екземпляр бази даних для отримання
            //її функціональності
            _context = context;
        }

        //метод отримання всіх даних
        public IEnumerable<Seedling> GetAll()
        => _context.Seedlings; //повертаємо колекцію саджанців
        //метод отримання одного саджанця за айді

        public Seedling GetById(int? id)//параметр, який буде передаватися
        => GetAll().FirstOrDefault(x => x.Id == id);//пошук і повернення   

        //метод додавання нового саджанця
        public async Task AddAsync(Seedling newSeedling)
        {
            //додавання саджанця
            await _context.Seedlings.AddAsync(newSeedling);
            //збереження змін
            await _context.SaveChangesAsync();
        }

        //метод оновлення даних існуючого саджанця
        public async Task UpdateAsync(Seedling seedling)
        {
            //оновлюємо дані саджанця
            _context.Seedlings.Update(seedling);
            //зберігаємо зміни
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(int id)
        {   //шукаємо саджанець за айді
            var seedling = GetById(id);
            //якщо саджанець був знайдений
            if(seedling != null)
            {    //видаляємо його
                _context.Seedlings.Remove(seedling);
                //зберігаємо зміни
                await _context.SaveChangesAsync();
                //повертаємо істинність
                return true;
            }
            else
            {//якщо саджанця не знайдено - повертаємо хибність
                return false;
            }
            
        }

    }
}
